using MongoDB.Driver;
using DongThapHelpdesk.Api.Configurations;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Data;

public class DatabaseSeeder
{
    private readonly IMongoDatabase _database;

    public DatabaseSeeder(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public async Task ClearAllAsync()
    {
        await _database.DropCollectionAsync("users");
        await _database.DropCollectionAsync("tickets");
        await _database.DropCollectionAsync("incident_categories");
        await _database.DropCollectionAsync("sla_policies");
        await _database.DropCollectionAsync("holidays");
        Console.WriteLine("✓ Đã xóa toàn bộ dữ liệu");
    }

    public async Task SeedAsync()
    {
        await SeedUsersAsync();
        await SeedIncidentCategoriesAsync();
        await SeedSlaPoliciesAsync();
        await SeedTicketsAsync();
        await SeedHolidaysAsync();
    }

    // ── 1. Seed Users ─────────────────────────────────────────
    private async Task SeedUsersAsync()
    {
        var collection = _database
            .GetCollection<AppUser>("users");

        if (await collection.CountDocumentsAsync(_ => true) > 0)
        {
            Console.WriteLine("⚠ Users đã có dữ liệu, bỏ qua seed");
            return;
        }

        var users = new List<AppUser>
        {
            // ── Admin ──────────────────────────────────────
            new()
            {
                FullName = "Quản trị viên",
                Email = "admin@dongthap.gov.vn",
                PhoneNumber = "0270000000",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = null
            },

            // ── Dispatcher ────────────────────────────────
            new()
            {
                FullName = "Nguyễn Văn Bình",
                Email = "binhnv@dongthap.gov.vn",
                PhoneNumber = "0901111111",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Dispatcher@123"),
                Role = UserRole.Dispatcher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = null
            },

            // ── Assignee ──────────────────────────────────
            new()
            {
                FullName = "Trần Thị Cẩm",
                Email = "camtt@dongthap.gov.vn",
                PhoneNumber = "0902222222",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Assignee@123"),
                Role = UserRole.Assignee,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = null
            },

            // ── Manager ───────────────────────────────────
            new()
            {
                FullName = "Lê Văn Dũng",
                Email = "dunglv@dongthap.gov.vn",
                PhoneNumber = "0903333333",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Manager@123"),
                Role = UserRole.Manager,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = null
            },

            // ── Citizen 1 ─────────────────────────────────
            new()
            {
                FullName = "Phạm Văn Em",
                Email = "empv@gmail.com",
                PhoneNumber = "0904444444",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Citizen@123"),
                Role = UserRole.Citizen,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = new CitizenProfile
                {
                    Address = "123 Nguyễn Huệ",
                    Ward = "Phường 1",
                    District = "TP. Cao Lãnh",
                    TotalPoints = 25,
                    CurrentMonthPoints = 15,
                    CurrentQuarterPoints = 25,
                    TotalTicketsSubmitted = 3,
                    TotalTicketsApproved = 2,
                    TotalTicketsRejected = 1,
                    LastActivityAt = DateTime.UtcNow
                }
            },

            // ── Citizen 2 ─────────────────────────────────
            new()
            {
                FullName = "Võ Thị Phương",
                Email = "phuongvt@gmail.com",
                PhoneNumber = "0905555555",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Citizen@123"),
                Role = UserRole.Citizen,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CitizenProfile = new CitizenProfile
                {
                    Address = "456 Trần Hưng Đạo",
                    Ward = "Phường 2",
                    District = "TP. Cao Lãnh",
                    TotalPoints = 40,
                    CurrentMonthPoints = 20,
                    CurrentQuarterPoints = 40,
                    TotalTicketsSubmitted = 5,
                    TotalTicketsApproved = 4,
                    TotalTicketsRejected = 1,
                    LastActivityAt = DateTime.UtcNow
                }
            }
        };

        await collection.InsertManyAsync(users);
        Console.WriteLine("✓ Users seeded thành công");
    }

    // ── 2. Seed Incident Categories ───────────────────────────
    private async Task SeedIncidentCategoriesAsync()
    {
        var collection = _database
            .GetCollection<IncidentCategory>("incident_categories");

        if (await collection.CountDocumentsAsync(_ => true) > 0)
        {
            Console.WriteLine(
                "⚠ IncidentCategories đã có dữ liệu, bỏ qua seed");
            return;
        }

        var categories = new List<IncidentCategory>
        {
            // ── Danh mục gốc ──────────────────────────────
            new()
            {
                Name = "Môi trường",
                Code = "MT",
                DefaultSlaHours = 72,
                IsActive = true
            },
            new()
            {
                Name = "Giao thông",
                Code = "GT",
                DefaultSlaHours = 48,
                IsActive = true
            },
            new()
            {
                Name = "An ninh trật tự",
                Code = "AN",
                DefaultSlaHours = 24,
                IsActive = true
            },
            new()
            {
                Name = "Hạ tầng đô thị",
                Code = "HT",
                DefaultSlaHours = 96,
                IsActive = true
            },
            new()
            {
                Name = "Thủ tục hành chính",
                Code = "HC",
                DefaultSlaHours = 120,
                IsActive = true
            }
        };

        await collection.InsertManyAsync(categories);

        // Lấy ID các danh mục gốc để tạo danh mục con
        var mtId = (await collection
            .Find(c => c.Code == "MT")
            .FirstOrDefaultAsync()).Id;

        var gtId = (await collection
            .Find(c => c.Code == "GT")
            .FirstOrDefaultAsync()).Id;

        // ── Danh mục con ──────────────────────────────────
        var subCategories = new List<IncidentCategory>
        {
            // Con của Môi trường
            new()
            {
                Name = "Rác thải",
                Code = "MT-RAC",
                ParentCategoryId = mtId,
                DefaultSlaHours = 48,
                IsActive = true
            },
            new()
            {
                Name = "Ô nhiễm nguồn nước",
                Code = "MT-NUOC",
                ParentCategoryId = mtId,
                DefaultSlaHours = 72,
                IsActive = true
            },
            new()
            {
                Name = "Khói bụi",
                Code = "MT-KHOI",
                ParentCategoryId = mtId,
                DefaultSlaHours = 48,
                IsActive = true
            },

            // Con của Giao thông
            new()
            {
                Name = "Ổ gà đường hỏng",
                Code = "GT-OGA",
                ParentCategoryId = gtId,
                DefaultSlaHours = 48,
                IsActive = true
            },
            new()
            {
                Name = "Đèn đường hỏng",
                Code = "GT-DEN",
                ParentCategoryId = gtId,
                DefaultSlaHours = 24,
                IsActive = true
            },
            new()
            {
                Name = "Biển báo hỏng",
                Code = "GT-BIEN",
                ParentCategoryId = gtId,
                DefaultSlaHours = 48,
                IsActive = true
            }
        };

        await collection.InsertManyAsync(subCategories);
        Console.WriteLine("✓ IncidentCategories seeded thành công");
    }

    // ── 3. Seed SLA Policies ──────────────────────────────────
    private async Task SeedSlaPoliciesAsync()
    {
        var slaCollection = _database
            .GetCollection<SlaPolicy>("sla_policies");

        if (await slaCollection
            .CountDocumentsAsync(_ => true) > 0)
        {
            Console.WriteLine(
                "⚠ SlaPolicies đã có dữ liệu, bỏ qua seed");
            return;
        }

        var categoryCollection = _database
            .GetCollection<IncidentCategory>("incident_categories");

        var categories = await categoryCollection
            .Find(_ => true).ToListAsync();

        var policies = categories.Select(c => new SlaPolicy
        {
            CategoryId = c.Id,
            ResolutionHours = c.DefaultSlaHours,
            WarningThresholdPercent = 80,
            ExcludeWeekends = true,
            ExcludeHolidays = true,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await slaCollection.InsertManyAsync(policies);
        Console.WriteLine("✓ SlaPolicies seeded thành công");
    }

    // ── 4. Seed Tickets ───────────────────────────────────────
    private async Task SeedTicketsAsync()
    {
        var ticketCollection = _database
            .GetCollection<Ticket>("tickets");

        if (await ticketCollection
            .CountDocumentsAsync(_ => true) > 0)
        {
            Console.WriteLine(
                "⚠ Tickets đã có dữ liệu, bỏ qua seed");
            return;
        }

        // Lấy ID category để gán cho ticket
        var categoryCollection = _database
            .GetCollection<IncidentCategory>("incident_categories");

        var gtOga = await categoryCollection
            .Find(c => c.Code == "GT-OGA")
            .FirstOrDefaultAsync();

        var mtRac = await categoryCollection
            .Find(c => c.Code == "MT-RAC")
            .FirstOrDefaultAsync();

        // Lấy ID citizen để gán cho ticket
        var userCollection = _database
            .GetCollection<AppUser>("users");

        var citizen1 = await userCollection
            .Find(u => u.PhoneNumber == "0904444444")
            .FirstOrDefaultAsync();

        var citizen2 = await userCollection
            .Find(u => u.PhoneNumber == "0905555555")
            .FirstOrDefaultAsync();

        var tickets = new List<Ticket>
        {
            // ── Ticket 1: New ─────────────────────────────
            new()
            {
                TicketCode = "PA-032026-001",
                Title = "Ổ gà lớn trên đường Nguyễn Huệ",
                Description = "Có một ổ gà sâu khoảng 20cm " +
                    "gây nguy hiểm cho người đi xe máy.",
                Status = TicketStatus.New,
                Priority = TicketPriority.High,
                CategoryId = gtOga?.Id,
                Location = new GeoLocation
                {
                    Type = "Point",
                    Coordinates = new[] { 105.9100, 10.3397 },
                    Address = "Đường Nguyễn Huệ, " +
                        "TP. Cao Lãnh, Đồng Tháp"
                },
                Attachments = new List<Attachment>(),
                ReporterId = citizen1?.Id,
                IsSlaBreached = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },

            // ── Ticket 2: InProgress ──────────────────────
            new()
            {
                TicketCode = "PA-032026-002",
                Title = "Rác thải tràn lan tại chợ Phường 2",
                Description = "Khu vực chợ Phường 2 có rác " +
                    "thải chưa được thu gom nhiều ngày.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Normal,
                CategoryId = mtRac?.Id,
                Location = new GeoLocation
                {
                    Type = "Point",
                    Coordinates = new[] { 105.9150, 10.3420 },
                    Address = "Chợ Phường 2, " +
                        "TP. Cao Lãnh, Đồng Tháp"
                },
                Attachments = new List<Attachment>(),
                ReporterId = citizen2?.Id,
                SlaDeadline = DateTime.UtcNow.AddHours(24),
                IsSlaBreached = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // ── Ticket 3: Closed ──────────────────────────
            new()
            {
                TicketCode = "PA-032026-003",
                Title = "Đèn đường hỏng tại hẻm 5",
                Description = "Đèn đường tại hẻm 5 " +
                    "đường Lý Thường Kiệt bị hỏng 3 ngày.",
                Status = TicketStatus.Closed,
                Priority = TicketPriority.Normal,
                CategoryId = gtOga?.Id,
                Location = new GeoLocation
                {
                    Type = "Point",
                    Coordinates = new[] { 105.9080, 10.3380 },
                    Address = "Hẻm 5, Lý Thường Kiệt, " +
                        "TP. Cao Lãnh, Đồng Tháp"
                },
                Attachments = new List<Attachment>(),
                ReporterId = citizen1?.Id,
                IsSlaBreached = false,
                ClosedAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },

            // ── Ticket 4: Rejected ────────────────────────
            new()
            {
                TicketCode = "PA-032026-004",
                Title = "Test spam",
                Description = "Báo cáo không hợp lệ",
                Status = TicketStatus.Rejected,
                Priority = TicketPriority.Low,
                CategoryId = mtRac?.Id,
                Location = null,
                Attachments = new List<Attachment>(),
                AnonymousName = "Ẩn danh",
                AnonymousPhone = "0909999999",
                IsSlaBreached = false,
                RejectionReason = "Báo cáo không có " +
                    "thông tin và hình ảnh minh chứng",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await ticketCollection.InsertManyAsync(tickets);
        Console.WriteLine("✓ Tickets seeded thành công");
    }

    // ── 5. Seed Holidays ──────────────────────────────────────
    private async Task SeedHolidaysAsync()
    {
        var collection = _database
            .GetCollection<Holiday>("holidays");

        if (await collection.CountDocumentsAsync(_ => true) > 0)
        {
            Console.WriteLine(
                "⚠ Holidays đã có dữ liệu, bỏ qua seed");
            return;
        }

        var holidays = new List<Holiday>
        {
            new()
            {
                Date = new DateTime(2026, 1, 1),
                Name = "Tết Dương lịch",
                IsRecurringYearly = true
            },
            new()
            {
                Date = new DateTime(2026, 4, 30),
                Name = "Ngày Giải phóng miền Nam",
                IsRecurringYearly = true
            },
            new()
            {
                Date = new DateTime(2026, 5, 1),
                Name = "Ngày Quốc tế Lao động",
                IsRecurringYearly = true
            },
            new()
            {
                Date = new DateTime(2026, 9, 2),
                Name = "Ngày Quốc khánh",
                IsRecurringYearly = true
            },
            new()
            {
                Date = new DateTime(2026, 4, 18),
                Name = "Giỗ Tổ Hùng Vương",
                IsRecurringYearly = true
            }
        };

        await collection.InsertManyAsync(holidays);
        Console.WriteLine("✓ Holidays seeded thành công");
    }
}