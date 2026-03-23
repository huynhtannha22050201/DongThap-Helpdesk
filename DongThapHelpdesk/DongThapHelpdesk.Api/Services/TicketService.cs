using DongThapHelpdesk.Api.DTOs.Ticket;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;
using DongThapHelpdesk.Api.Services;
using MongoDB.Driver;

namespace DongThapHelpdesk.Api.Services;

public class TicketService
{
    private readonly TicketRepository _ticketRepo;
    private readonly UserRepository _userRepo;
    private readonly CategoryRepository _categoryRepo;
    private readonly DepartmentRepository _departmentRepo;
    private readonly RatingRepository _ratingRepo;
    private readonly TicketActivityRepository _activityRepo;
    private readonly SmsService _smsService;
    private readonly PointService _pointService;
    private readonly TicketCodeGenerator _codeGenerator;
    private readonly FileUploadService _fileUploadService;

    public TicketService(
        TicketRepository ticketRepo,
        UserRepository userRepo,
        CategoryRepository categoryRepo,
        DepartmentRepository departmentRepo,
        RatingRepository ratingRepo,
        TicketActivityRepository activityRepo,
        SmsService smsService,
        PointService pointService,
        TicketCodeGenerator codeGenerator,
        FileUploadService fileUploadService)
    {
        _ticketRepo = ticketRepo;
        _userRepo = userRepo;
        _categoryRepo = categoryRepo;
        _departmentRepo = departmentRepo;
        _ratingRepo = ratingRepo;
        _activityRepo = activityRepo;
        _smsService = smsService;
        _pointService = pointService;
        _codeGenerator = codeGenerator;
        _fileUploadService = fileUploadService;
    }

    // ── Lấy dữ liệu ──────────────────────────────────────
    public async Task<List<TicketResponse>> GetAllAsync()
    {
        var tickets = await _ticketRepo.GetAllAsync();
        return await MapToResponseListAsync(tickets);
    }

    public async Task<TicketResponse?> GetByIdAsync(string id)
    {
        var ticket = await _ticketRepo.GetByIdAsync(id);
        if (ticket == null) return null;
        return await MapToResponseAsync(ticket);
    }

    public async Task<TicketResponse?> GetByCodeAsync(
        string code)
    {
        var ticket = await _ticketRepo.GetByCodeAsync(code);
        if (ticket == null) return null;
        return await MapToResponseAsync(ticket);
    }

    public async Task<List<TicketResponse>>
        GetByCitizenIdAsync(string citizenId)
    {
        var filter = Builders<Ticket>.Filter
            .Eq(t => t.ReporterId, citizenId);
        var tickets = await _ticketRepo
            .GetByFilterAsync(filter);
        return await MapToResponseListAsync(tickets);
    }

    public async Task<List<TicketResponse>>
        GetByDepartmentAsync(string departmentId)
    {
        var filter = Builders<Ticket>.Filter
            .Eq(t => t.AssignedDepartmentId, departmentId);
        var tickets = await _ticketRepo
            .GetByFilterAsync(filter);
        return await MapToResponseListAsync(tickets);
    }

    public async Task<List<TicketActivity>>
        GetActivitiesAsync(string ticketId)
    {
        var filter = Builders<TicketActivity>.Filter
            .Eq(a => a.TicketId, ticketId);
        return await _activityRepo.GetByFilterAsync(filter);
    }

    // ── UC01: Gửi phản ánh ───────────────────────────────
    public async Task<(bool Success, string Message,
    TicketResponse? Data)> CreateAsync(
    CreateTicketRequest request)
    {
        // Nghiệp vụ: validate category
        var category = await _categoryRepo
            .GetByIdAsync(request.CategoryId);
        if (category == null)
            return (false, "Danh mục không tồn tại", null);

        // Nghiệp vụ: chuẩn hóa SĐT
        var normalizedPhone = NormalizePhone(
            request.ReporterPhone);

        // Nghiệp vụ: kiểm tra tài khoản hiện có
        var existingUser = await _userRepo
            .GetByPhoneNumberAsync(normalizedPhone);

        // Nghiệp vụ: upload ảnh trước khi tạo ticket
        var attachments = new List<Attachment>();
        if (request.Files != null && request.Files.Any())
        {
            var (uploadSuccess, uploadMessage, uploadedFiles) =
                await _fileUploadService.UploadFilesAsync(
                    request.Files, "tickets");

            if (!uploadSuccess)
                return (false,
                    $"Lỗi upload ảnh: {uploadMessage}",
                    null);

            attachments = uploadedFiles;
        }

        // Tạo ticket kèm ảnh đã upload
        var ticket = new Ticket
        {
            TicketCode = await _codeGenerator.GenerateAsync(),
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Status = TicketStatus.New,
            Priority = TicketPriority.Normal,
            SlaHours = category.DefaultSlaHours,
            ReporterName = request.ReporterName,
            ReporterPhone = normalizedPhone,
            ReporterAddress = request.ReporterAddress,
            ReporterWard = request.ReporterWard,
            ReporterDistrict = request.ReporterDistrict,
            ReporterId = existingUser?.Id,
            IsAccountCreated = existingUser != null,
            Location = BuildLocation(request),
            Attachments = attachments, // ← ảnh đã upload
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _ticketRepo.CreateAsync(ticket);

        // Nghiệp vụ: cập nhật số lượng báo cáo
        if (existingUser?.CitizenProfile != null)
        {
            existingUser.CitizenProfile.TotalTicketsSubmitted++;
            existingUser.CitizenProfile.LastActivityAt
                = DateTime.UtcNow;
            await _userRepo.UpdateAsync(existingUser);
        }

        // Ghi activity
        await LogActivityAsync(
            ticket.Id, null,
            ActivityType.Created,
            null, TicketStatus.New,
            $"Phản ánh mới được gửi lên hệ thống" +
            $"{(attachments.Any() ? $" kèm {attachments.Count} hình ảnh" : "")}");

        return (true, "Gửi phản ánh thành công",
            await MapToResponseAsync(ticket));
    }

    // ── UC05: Dispatcher duyệt ticket ────────────────────
    public async Task<(bool Success, string Message)>
        ApproveAsync(
        string ticketId,
        string dispatcherId,
        ApproveTicketRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        // Nghiệp vụ: kiểm tra trạng thái hợp lệ
        if (ticket.Status != TicketStatus.New)
            return (false,
                "Chỉ có thể duyệt phản ánh ở trạng thái Mới");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.UnderReview;
        ticket.Priority = request.Priority;
        ticket.ReviewedByDispatcherId = dispatcherId;
        ticket.ReviewedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        // Nghiệp vụ: tạo tài khoản nếu chưa có
        await CreateAccountIfNotExistsAsync(ticket);

        // Nghiệp vụ: cộng điểm
        if (!string.IsNullOrEmpty(ticket.ReporterId))
            await _pointService.AddPointsAsync(
                ticket.ReporterId,
                PointAction.TicketApproved,
                ticket.Id);

        // Ghi activity
        await LogActivityAsync(
            ticket.Id,
            dispatcherId,
            ActivityType.Approved,
            oldStatus,
            ticket.Status,
            "Phản ánh được duyệt hợp lệ");

        // Gửi SMS
        await _smsService.SendTicketApprovedAsync(
            ticket.ReporterPhone, ticket.TicketCode);

        return (true, "Duyệt phản ánh thành công");
    }

    // ── UC05: Dispatcher từ chối ticket ──────────────────
    public async Task<(bool Success, string Message)>
        RejectAsync(
        string ticketId,
        string dispatcherId,
        RejectTicketRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.New &&
            ticket.Status != TicketStatus.UnderReview)
            return (false, "Không thể từ chối ở trạng thái này");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Rejected;
        ticket.RejectionReason = request.Reason;
        ticket.ReviewedByDispatcherId = dispatcherId;
        ticket.ReviewedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        // Nghiệp vụ: trừ điểm nếu có tài khoản
        if (!string.IsNullOrEmpty(ticket.ReporterId))
            await _pointService.AddPointsAsync(
                ticket.ReporterId,
                PointAction.TicketRejected,
                ticket.Id,
                request.Reason);

        await LogActivityAsync(
            ticket.Id,
            dispatcherId,
            ActivityType.Rejected,
            oldStatus,
            ticket.Status,
            $"Từ chối: {request.Reason}");

        return (true, "Từ chối phản ánh thành công");
    }

    // ── UC06: Dispatcher giao việc ────────────────────────
    public async Task<(bool Success, string Message)>
        AssignAsync(
        string ticketId,
        string dispatcherId,
        AssignTicketRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.UnderReview)
            return (false,
                "Chỉ có thể giao việc ở trạng thái Đang kiểm duyệt");

        // Nghiệp vụ: kiểm tra đơn vị tồn tại
        var department = await _departmentRepo
            .GetByIdAsync(request.DepartmentId);
        if (department == null)
            return (false, "Đơn vị không tồn tại");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Assigned;
        ticket.AssignedDepartmentId = request.DepartmentId;
        ticket.AssignedUserId = request.AssignedUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        await LogActivityAsync(
            ticket.Id,
            dispatcherId,
            ActivityType.Assigned,
            oldStatus,
            ticket.Status,
            request.Note ?? $"Giao cho {department.Name}");

        return (true, "Giao việc thành công");
    }

    // ── UC09: Assignee tiếp nhận ──────────────────────────
    public async Task<(bool Success, string Message)>
        StartProgressAsync(
        string ticketId, string assigneeId)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.Assigned)
            return (false,
                "Chỉ có thể tiếp nhận phản ánh đã được giao");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.InProgress;

        // Nghiệp vụ: tính SLA deadline từ category
        ticket.SlaDeadline = DateTime.UtcNow
            .AddHours(ticket.SlaHours);
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        await LogActivityAsync(
            ticket.Id,
            assigneeId,
            ActivityType.InProgress,
            oldStatus,
            ticket.Status,
            "Cán bộ đã tiếp nhận và bắt đầu xử lý");

        return (true, "Tiếp nhận thành công");
    }

    // ── UC10: Assignee báo cáo kết quả ───────────────────
    public async Task<(bool Success, string Message)>
        SubmitResultAsync(
        string ticketId,
        string assigneeId,
        SubmitResultRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.InProgress)
            return (false,
                "Chỉ có thể báo cáo kết quả khi đang xử lý");

        // Upload ảnh minh chứng
        var attachmentUrls = new List<string>();
        if (request.Files != null && request.Files.Any())
        {
            var (uploadSuccess, uploadMessage, uploaded) =
                await _fileUploadService.UploadFilesAsync(
                    request.Files, "results");

            if (!uploadSuccess)
                return (false,
                    $"Lỗi upload ảnh: {uploadMessage}");

            // Thêm ảnh minh chứng vào ticket
            ticket.Attachments.AddRange(uploaded);
            attachmentUrls = uploaded
                .Select(a => a.Url).ToList();
        }

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.PendingVerification;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        await LogActivityAsync(
            ticket.Id,
            assigneeId,
            ActivityType.ResultReported,
            oldStatus,
            ticket.Status,
            request.ResultNote,
            attachmentUrls);

        return (true, "Báo cáo kết quả thành công");
    }

    // ── UC11: Assignee chuyển trả ─────────────────────────
    public async Task<(bool Success, string Message)>
        ReassignAsync(
        string ticketId,
        string assigneeId,
        ReassignTicketRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.Assigned &&
            ticket.Status != TicketStatus.InProgress)
            return (false,
                "Không thể chuyển trả ở trạng thái này");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.UnderReview;
        ticket.AssignedDepartmentId = null;
        ticket.AssignedUserId = null;
        ticket.SlaDeadline = null;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        await LogActivityAsync(
            ticket.Id,
            assigneeId,
            ActivityType.Reassigned,
            oldStatus,
            ticket.Status,
            $"Chuyển trả: {request.Reason}");

        return (true, "Chuyển trả hồ sơ thành công");
    }

    // ── UC07: Dispatcher đóng ticket ──────────────────────
    public async Task<(bool Success, string Message)>
        CloseAsync(string ticketId, string dispatcherId)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        if (ticket.Status != TicketStatus.PendingVerification)
            return (false,
                "Chỉ có thể đóng phản ánh đang chờ xác nhận");

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);

        // Nghiệp vụ: cộng điểm đóng ticket
        if (!string.IsNullOrEmpty(ticket.ReporterId))
            await _pointService.AddPointsAsync(
                ticket.ReporterId,
                PointAction.TicketClosed,
                ticket.Id);

        await LogActivityAsync(
            ticket.Id,
            dispatcherId,
            ActivityType.Closed,
            oldStatus,
            ticket.Status,
            "Đã xác nhận kết quả và đóng phản ánh");

        // Gửi SMS thông báo
        await _smsService.SendTicketClosedAsync(
            ticket.ReporterPhone, ticket.TicketCode);

        return (true, "Đóng phản ánh thành công");
    }

    public async Task<(bool Success, string Message)>
        RateAsync(
        string ticketId,
        string citizenId,
        RatingRequest request)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        if (ticket == null)
            return (false, "Không tìm thấy phản ánh");

        // Nghiệp vụ: chỉ đánh giá ticket đã đóng
        if (ticket.Status != TicketStatus.Closed)
            return (false,
                "Chỉ có thể đánh giá phản ánh đã đóng");

        // Nghiệp vụ: kiểm tra đúng người báo cáo
        if (ticket.ReporterId != citizenId)
            return (false,
                "Bạn không có quyền đánh giá phản ánh này");

        // Nghiệp vụ: kiểm tra đã đánh giá chưa
        var existing = await _ratingRepo
            .GetByTicketIdAsync(ticketId);
        if (existing != null)
            return (false, "Bạn đã đánh giá phản ánh này rồi");

        // Nghiệp vụ: validate số sao
        if (request.Stars < 1 || request.Stars > 5)
            return (false, "Số sao phải từ 1 đến 5");

        var rating = new Rating
        {
            TicketId = ticketId,
            CitizenId = citizenId,
            Stars = request.Stars,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepo.CreateAsync(rating);

        // Nghiệp vụ: cộng điểm đánh giá
        await _pointService.AddPointsAsync(
            citizenId,
            PointAction.RatingSubmitted,
            ticketId,
            "Đã đánh giá kết quả xử lý");

        return (true, "Đánh giá thành công");
    }

    // ── Helper: Tự động tạo tài khoản ────────────────────
    private async Task CreateAccountIfNotExistsAsync(
        Ticket ticket)
    {
        // Đã có tài khoản rồi thì bỏ qua
        if (!string.IsNullOrEmpty(ticket.ReporterId)) return;
        if (ticket.IsAccountCreated) return;

        // Kiểm tra SĐT đã có tài khoản chưa
        var existingUser = await _userRepo
            .GetByPhoneNumberAsync(ticket.ReporterPhone);

        if (existingUser != null)
        {
            // Liên kết ticket với tài khoản cũ
            ticket.ReporterId = existingUser.Id;
            ticket.IsAccountCreated = true;
            await _ticketRepo.UpdateAsync(ticket);
            return;
        }

        // Tạo mật khẩu ngẫu nhiên 8 ký tự
        var password = GenerateRandomPassword();

        // Tạo tài khoản mới
        var newUser = new AppUser
        {
            FullName = ticket.ReporterName,
            PhoneNumber = ticket.ReporterPhone,
            Email = "",
            PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(password),
            Role = UserRole.Citizen,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CitizenProfile = new CitizenProfile
            {
                Address = ticket.ReporterAddress,
                Ward = ticket.ReporterWard,
                District = ticket.ReporterDistrict,
                TotalPoints = 10,
                // Cộng 10 điểm cho báo cáo đầu tiên được duyệt
                CurrentMonthPoints = 10,
                CurrentQuarterPoints = 10,
                TotalTicketsSubmitted = 1,
                TotalTicketsApproved = 0,
                TotalTicketsRejected = 0,
                LastActivityAt = DateTime.UtcNow
            }
        };

        await _userRepo.CreateAsync(newUser);

        // Cập nhật ticket liên kết tài khoản mới
        ticket.ReporterId = newUser.Id;
        ticket.IsAccountCreated = true;
        await _ticketRepo.UpdateAsync(ticket);

        // Gửi SMS thông báo tài khoản
        await _smsService.SendAccountCreatedAsync(
            ticket.ReporterPhone, password);

        // Gửi SMS báo cáo được duyệt
        await _smsService.SendTicketApprovedAsync(
            ticket.ReporterPhone, ticket.TicketCode);
    }

    // ── Helper ─────────────────

    private async Task LogActivityAsync(
        string ticketId,
        string? actorId,
        ActivityType actionType,
        TicketStatus? oldStatus,
        TicketStatus? newStatus,
        string? comment = null,
        List<string>? attachmentUrls = null)
    {
        var activity = new TicketActivity
        {
            TicketId = ticketId,
            ActorId = actorId,
            ActionType = actionType,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = comment,
            AttachmentUrls = attachmentUrls ?? new(),
            CreatedAt = DateTime.UtcNow
        };

        await _activityRepo.CreateAsync(activity);
    }

    private async Task<List<TicketResponse>>
        MapToResponseListAsync(List<Ticket> tickets)
    {
        var result = new List<TicketResponse>();
        foreach (var t in tickets)
            result.Add(await MapToResponseAsync(t));
        return result;
    }

    private async Task<TicketResponse> MapToResponseAsync(
        Ticket t)
    {
        var category = t.CategoryId != null
            ? await _categoryRepo.GetByIdAsync(t.CategoryId)
            : null;

        var department = t.AssignedDepartmentId != null
            ? await _departmentRepo.GetByIdAsync(
                t.AssignedDepartmentId)
            : null;

        var assignedUser = t.AssignedUserId != null
            ? await _userRepo.GetByIdAsync(t.AssignedUserId)
            : null;

        return new TicketResponse
        {
            Id = t.Id,
            TicketCode = t.TicketCode,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            CategoryId = t.CategoryId,
            CategoryName = category?.Name,
            Location = t.Location != null
                ? new LocationResponse
                {
                    Longitude = t.Location.Longitude,
                    Latitude = t.Location.Latitude,
                    Address = t.Location.Address
                }
                : null,
            Attachments = t.Attachments.Select(a =>
                new AttachmentResponse
                {
                    FileName = a.FileName,
                    Url = a.Url,
                    MimeType = a.MimeType
                }).ToList(),
            ReporterName = t.ReporterName,
            ReporterPhone = t.ReporterPhone,
            ReporterAddress = t.ReporterAddress,
            ReporterWard = t.ReporterWard,
            ReporterDistrict = t.ReporterDistrict,
            AssignedDepartmentId = t.AssignedDepartmentId,
            AssignedDepartmentName = department?.Name,
            AssignedUserName = assignedUser?.FullName,
            SlaHours = t.SlaHours,
            SlaDeadline = t.SlaDeadline,
            IsSlaBreached = t.IsSlaBreached,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ClosedAt = t.ClosedAt,
            RejectionReason = t.RejectionReason
        };
    }

    private static GeoLocation? BuildLocation(
        CreateTicketRequest request)
    {
        if (!request.Latitude.HasValue ||
            !request.Longitude.HasValue) return null;

        return new GeoLocation
        {
            Type = "Point",
            Coordinates = new[]
            {
                request.Longitude.Value,
                request.Latitude.Value
            },
            Address = request.Address
        };
    }

    private static string GenerateRandomPassword()
    {
        const string chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz" +
            "23456789@#";
        var random = new Random();
        return new string(Enumerable
            .Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

    // ── Helper: Chuẩn hóa SĐT ────────────────────────────
    private static string NormalizePhone(string phone)
    {
        phone = phone.Replace(" ", "").Replace("-", "");
        if (phone.StartsWith("+84"))
            return "0" + phone[3..];
        if (phone.StartsWith("84") && phone.Length == 11)
            return "0" + phone[2..];
        return phone;
    }
}