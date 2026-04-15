using MongoDB.Driver;
using MongoDB.Bson;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpDesk.Api.Models; // PointPolicy namespace

namespace DongThapHelpdesk.Api.Data;

/// <summary>
/// Dev-only: Xóa sạch toàn bộ DB → Seed dữ liệu mẫu mỗi lần khởi động.
/// Dữ liệu mô phỏng thực tế tỉnh Đồng Tháp (mới) sau sáp nhập 01/07/2025.
/// </summary>
public class DatabaseSeeder
{
    private readonly MongoDbContext _context;

    // Password mặc định cho tất cả tài khoản seed: "123456"
    private static readonly string DefaultPasswordHash =
        BCrypt.Net.BCrypt.HashPassword("123456");

    public DatabaseSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // ══════════════════════════════════════════════════
        // BƯỚC 0: XÓA SẠCH TOÀN BỘ DỮ LIỆU CŨ
        // ══════════════════════════════════════════════════
        await DropAllCollectionsAsync();

        // ══════════════════════════════════════════════════
        // BƯỚC 1: SEED DANH MỤC PHẢN ÁNH
        // ══════════════════════════════════════════════════
        var categories = GetCategories();
        await _context.GetCollection<IncidentCategory>()
            .InsertManyAsync(categories);

        // ══════════════════════════════════════════════════
        // BƯỚC 2: SEED ĐƠN VỊ HÀNH CHÍNH
        // Cấp tỉnh (1) + Cấp xã/phường (102) = 103 đơn vị
        // ══════════════════════════════════════════════════
        var provinceDept = GetProvinceDepartment();
        await _context.GetCollection<Department>()
            .InsertOneAsync(provinceDept);

        var communeDepts = GetCommuneDepartments(provinceDept.Id);
        await _context.GetCollection<Department>()
            .InsertManyAsync(communeDepts);

        // ══════════════════════════════════════════════════
        // BƯỚC 3: SEED NGƯỜI DÙNG
        // ══════════════════════════════════════════════════
        var admin = GetAdmin();
        var manager = GetManager();
        var dispatchers = GetDispatchers(provinceDept.Id);
        var assignees = GetAssignees(communeDepts);
        var citizens = GetCitizens();

        var allUsers = new List<AppUser> { admin, manager };
        allUsers.AddRange(dispatchers);
        allUsers.AddRange(assignees);
        allUsers.AddRange(citizens);

        await _context.GetCollection<AppUser>()
            .InsertManyAsync(allUsers);

        // Gắn ResponsibleUserId cho từng đơn vị xã/phường
        // Mỗi đơn vị có 1 Assignee làm chủ tịch phụ trách
        await LinkAssigneesToDepartmentsAsync(communeDepts, assignees);

        // ══════════════════════════════════════════════════
        // BƯỚC 4: SEED SLA POLICIES
        // ══════════════════════════════════════════════════
        var slaPolicies = GetSlaPolicies(categories);
        await _context.GetCollection<SlaPolicy>()
            .InsertManyAsync(slaPolicies);

        // ══════════════════════════════════════════════════
        // BƯỚC 5: SEED POINT POLICIES
        // ══════════════════════════════════════════════════
        var pointPolicies = GetPointPolicies();
        await _context.GetCollection<PointPolicy>()
            .InsertManyAsync(pointPolicies);

        // ══════════════════════════════════════════════════
        // BƯỚC 6: SEED NGÀY LỄ
        // ══════════════════════════════════════════════════
        var holidays = GetHolidays();
        await _context.GetCollection<Holiday>()
            .InsertManyAsync(holidays);

        // ══════════════════════════════════════════════════
        // BƯỚC 7: SEED TICKETS + ACTIVITIES + RATINGS
        // ══════════════════════════════════════════════════
        await SeedTicketsAsync(
            categories, communeDepts,
            dispatchers, assignees, citizens);

        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("  DATABASE SEEDER HOÀN TẤT!");
        Console.WriteLine($"  • {categories.Count} danh mục");
        Console.WriteLine($"  • {communeDepts.Count + 1} đơn vị (1 tỉnh + {communeDepts.Count} xã/phường)");
        Console.WriteLine($"  • {allUsers.Count} người dùng");
        Console.WriteLine("═══════════════════════════════════════");
    }

    // ══════════════════════════════════════════════════════
    // DROP ALL COLLECTIONS
    // ══════════════════════════════════════════════════════
    /// <summary>
    /// Xóa sạch tất cả documents trong mọi collection.
    /// Nếu MongoDbContext chưa có GetDatabase(), hãy thêm vào:
    ///   public IMongoDatabase GetDatabase() => _database;
    /// Hoặc dùng fallback DeleteMany bên dưới.
    /// </summary>
    private async Task DropAllCollectionsAsync()
    {
        // Cách 1: Nếu MongoDbContext đã expose GetDatabase()
        // var db = _context.GetDatabase();
        // var names = new[] { "users", "tickets", ... };
        // foreach (var n in names) await db.DropCollectionAsync(n);

        // Cách 2 (an toàn): Xóa documents qua GetCollection<T>()
        var emptyFilter = Builders<BsonDocument>.Filter.Empty;

        await _context.GetCollection<AppUser>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<Ticket>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<IncidentCategory>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<Department>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<Notification>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<TicketActivity>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<Rating>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<PointHistory>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<PointPolicy>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<SlaPolicy>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<Holiday>()
            .DeleteManyAsync(_ => true);
        await _context.GetCollection<AuditLog>()
            .DeleteManyAsync(_ => true);
    }

    // ══════════════════════════════════════════════════════
    // DANH MỤC PHẢN ÁNH — 12 lĩnh vực chính
    // ══════════════════════════════════════════════════════
    private static List<IncidentCategory> GetCategories()
    {
        return new List<IncidentCategory>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Giao thông",
                Code = "GT",
                Description = "Phản ánh về đường sá hư hỏng, ổ gà, cầu cống xuống cấp, " +
                    "hệ thống tín hiệu giao thông hỏng, lấn chiếm vỉa hè, " +
                    "lòng lề đường và trật tự an toàn giao thông",
                DefaultSlaHours = 168, // 7 ngày
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Môi trường",
                Code = "MT",
                Description = "Phản ánh về rác thải sinh hoạt, ô nhiễm nguồn nước, " +
                    "ô nhiễm không khí, tiếng ồn vượt mức, xả thải trái phép, " +
                    "đốt rơm rạ gây khói bụi và vi phạm vệ sinh môi trường",
                DefaultSlaHours = 120, // 5 ngày
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Hạ tầng đô thị",
                Code = "HT",
                Description = "Phản ánh về cấp thoát nước, điện chiếu sáng công cộng, " +
                    "công viên - cây xanh, vỉa hè - lề đường, " +
                    "hệ thống thoát nước và ngập úng cục bộ",
                DefaultSlaHours = 168,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "An ninh trật tự",
                Code = "ANTT",
                Description = "Phản ánh về an ninh khu dân cư, gây rối trật tự công cộng, " +
                    "tiếng ồn ban đêm, hoạt động kinh doanh trái phép, " +
                    "tụ tập gây mất an ninh và các hành vi vi phạm pháp luật",
                DefaultSlaHours = 72, // 3 ngày — khẩn cấp
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Y tế – Sức khỏe",
                Code = "YT",
                Description = "Phản ánh về vệ sinh an toàn thực phẩm, dịch bệnh, " +
                    "cơ sở y tế hoạt động không phép, " +
                    "phòng chống dịch bệnh và chất lượng dịch vụ y tế công",
                DefaultSlaHours = 48, // 2 ngày — ưu tiên cao
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Giáo dục – Đào tạo",
                Code = "GD",
                Description = "Phản ánh về cơ sở vật chất trường học, " +
                    "chất lượng giảng dạy, thu phí sai quy định, " +
                    "an toàn trường học và các vấn đề liên quan giáo dục",
                DefaultSlaHours = 120,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Xây dựng – Đất đai",
                Code = "XD",
                Description = "Phản ánh về xây dựng trái phép, vi phạm quy hoạch, " +
                    "tranh chấp đất đai, cấp giấy chứng nhận quyền sử dụng đất " +
                    "và các vấn đề liên quan đến nhà ở, xây dựng",
                DefaultSlaHours = 240, // 10 ngày — phức tạp
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Nông nghiệp – Thủy lợi",
                Code = "NN",
                Description = "Phản ánh về hệ thống kênh mương, đê điều, " +
                    "trạm bơm tưới tiêu, dịch bệnh cây trồng - vật nuôi, " +
                    "phân bón giả, thuốc bảo vệ thực vật kém chất lượng",
                DefaultSlaHours = 168,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Văn hóa – Xã hội",
                Code = "VH",
                Description = "Phản ánh về hoạt động văn hóa, thể thao, " +
                    "chính sách xã hội, bảo trợ xã hội, " +
                    "bảo tồn di tích và các vấn đề liên quan đời sống văn hóa cộng đồng",
                DefaultSlaHours = 168,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Dịch vụ hành chính công",
                Code = "HC",
                Description = "Phản ánh về thái độ phục vụ của cán bộ công chức, " +
                    "thủ tục hành chính rườm rà, chậm trễ giải quyết hồ sơ, " +
                    "tiêu cực trong giải quyết thủ tục và dịch vụ công trực tuyến",
                DefaultSlaHours = 72,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Điện – Nước sinh hoạt",
                Code = "DN",
                Description = "Phản ánh về mất điện, mất nước kéo dài, " +
                    "chất lượng điện nước không đảm bảo, " +
                    "hóa đơn bất thường và sự cố đường dây điện, ống nước",
                DefaultSlaHours = 48,
                IsActive = true
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Khác",
                Code = "KHAC",
                Description = "Các phản ánh, kiến nghị khác không thuộc " +
                    "các lĩnh vực đã phân loại ở trên",
                DefaultSlaHours = 168,
                IsActive = true
            }
        };
    }

    // ══════════════════════════════════════════════════════
    // ĐƠN VỊ CẤP TỈNH — UBND tỉnh Đồng Tháp
    // ══════════════════════════════════════════════════════
    private static Department GetProvinceDepartment()
    {
        return new Department
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "UBND tỉnh Đồng Tháp",
            Code = "UBND-TINH",
            Level = DepartmentLevel.Province,
            ParentId = null, // Cấp cao nhất
            ResponsibleUserId = null, // Sẽ gắn Manager sau
            IsActive = true
        };
    }

    // ══════════════════════════════════════════════════════
    // 102 ĐƠN VỊ CẤP XÃ/PHƯỜNG SAU SÁP NHẬP
    // Theo Nghị quyết 1663/NQ-UBTVQH15 ngày 16/06/2025
    // ══════════════════════════════════════════════════════
    private static List<Department> GetCommuneDepartments(string provinceId)
    {
        // Danh sách 102 xã/phường (82 xã + 20 phường)
        // Format: (Tên, Mã code)
        var communeData = new (string Name, string Code)[]
        {
            // ── 82 XÃ ────────────────────────────────────────
            // Khu vực Đồng Tháp cũ (1-38)
            ("Xã Tân Hồng",          "XA-TANHONG"),
            ("Xã Tân Thành",         "XA-TANTHANH"),
            ("Xã Tân Hộ Cơ",         "XA-TANHOCO"),
            ("Xã An Phước",          "XA-ANPHUOC"),
            ("Xã Thường Phước",      "XA-THUONGPHUOC"),
            ("Xã Long Khánh",        "XA-LONGKHANH"),
            ("Xã Long Phú Thuận",    "XA-LONGPHUTHUAN"),
            ("Xã An Hòa",            "XA-ANHOA"),
            ("Xã Tam Nông",          "XA-TAMNONG"),
            ("Xã Phú Thọ",           "XA-PHUTHO"),
            ("Xã Tràm Chim",         "XA-TRAMCHIM"),
            ("Xã Phú Cường",         "XA-PHUCUONG"),
            ("Xã An Long",           "XA-ANLONG"),
            ("Xã Thanh Bình",        "XA-THANHBINH"),
            ("Xã Tân Thạnh",         "XA-TANTHANH-TB"),
            ("Xã Bình Thành",        "XA-BINHTHANH"),
            ("Xã Tân Long",          "XA-TANLONG"),
            ("Xã Tháp Mười",         "XA-THAPMUOI"),
            ("Xã Thanh Mỹ",          "XA-THANHMY"),
            ("Xã Mỹ Quí",            "XA-MYQUI"),
            ("Xã Đốc Binh Kiều",     "XA-DOCBINHKIEU"),
            ("Xã Trường Xuân",       "XA-TRUONGXUAN"),
            ("Xã Phương Thịnh",      "XA-PHUONGTHINH"),
            ("Xã Phong Mỹ",          "XA-PHONGMY"),
            ("Xã Ba Sao",            "XA-BASAO"),
            ("Xã Mỹ Thọ",            "XA-MYTHO-CL"),
            ("Xã Bình Hàng Trung",   "XA-BINHHANGTRUNG"),
            ("Xã Mỹ Hiệp",           "XA-MYHIEP"),
            ("Xã Mỹ An Hưng",        "XA-MYANHUNG"),
            ("Xã Tân Khánh Trung",   "XA-TANKHANHTRUNG"),
            ("Xã Lấp Vò",            "XA-LAPVO"),
            ("Xã Lai Vung",           "XA-LAIVUNG"),
            ("Xã Hòa Long",           "XA-HOALONG"),
            ("Xã Phong Hòa",          "XA-PHONGHOA"),
            ("Xã Tân Dương",          "XA-TANDUONG"),
            ("Xã Phú Hựu",            "XA-PHUHUU"),
            ("Xã Tân Nhuận Đông",    "XA-TANNHUANDONG"),
            ("Xã Tân Phú Trung",     "XA-TANPHUTRUNG"),

            // Khu vực Tiền Giang cũ (39-82)
            ("Xã Tân Phú",            "XA-TANPHU"),
            ("Xã Thanh Hưng",         "XA-THANHHUNG"),
            ("Xã An Hữu",             "XA-ANHUU"),
            ("Xã Mỹ Lợi",             "XA-MYLOI"),
            ("Xã Mỹ Đức Tây",        "XA-MYDUCTAY"),
            ("Xã Mỹ Thiện",           "XA-MYTHIEN"),
            ("Xã Hậu Mỹ",            "XA-HAUMY"),
            ("Xã Hội Cư",             "XA-HOICU"),
            ("Xã Cái Bè",             "XA-CAIBE"),
            ("Xã Mỹ Thành",           "XA-MYTHANH"),
            ("Xã Thạnh Phú",          "XA-THANHPHU"),
            ("Xã Bình Phú",           "XA-BINHPHU"),
            ("Xã Hiệp Đức",           "XA-HIEPDUC"),
            ("Xã Long Tiên",          "XA-LONGTIEN"),
            ("Xã Ngũ Hiệp",           "XA-NGUHIEP"),
            ("Xã Tân Phước 1",       "XA-TANPHUOC1"),
            ("Xã Tân Phước 2",       "XA-TANPHUOC2"),
            ("Xã Tân Phước 3",       "XA-TANPHUOC3"),
            ("Xã Hưng Thạnh",         "XA-HUNGTHANH"),
            ("Xã Tân Hương",          "XA-TANHUONG"),
            ("Xã Châu Thành",         "XA-CHAUTHANH"),
            ("Xã Long Hưng",          "XA-LONGHUNG"),
            ("Xã Long Định",          "XA-LONGDINH"),
            ("Xã Bình Trưng",         "XA-BINHTRUNG"),
            ("Xã Vĩnh Kim",           "XA-VINHKIM"),
            ("Xã Kim Sơn",            "XA-KIMSON"),
            ("Xã Mỹ Tịnh An",        "XA-MYTINHAN"),
            ("Xã Lương Hòa Lạc",     "XA-LUONGHOALAC"),
            ("Xã Tân Thuận Bình",     "XA-TANTHUANBINH"),
            ("Xã Chợ Gạo",            "XA-CHOGAO"),
            ("Xã An Thạnh Thủy",     "XA-ANTHANHTHUY"),
            ("Xã Bình Ninh",          "XA-BINHNINH"),
            ("Xã Vĩnh Bình",          "XA-VINHBINH"),
            ("Xã Đồng Sơn",           "XA-DONGSON"),
            ("Xã Phú Thành",          "XA-PHUTHANH"),
            ("Xã Long Bình",          "XA-LONGBINH"),
            ("Xã Vĩnh Hựu",           "XA-VINHHUU"),
            ("Xã Gò Công Đông",      "XA-GOCONGDONG"),
            ("Xã Tân Điền",           "XA-TANDIEN"),
            ("Xã Tân Hòa",            "XA-TANHOA"),
            ("Xã Tân Đông",           "XA-TANDONG"),
            ("Xã Gia Thuận",          "XA-GIATHUAN"),
            ("Xã Tân Thới",           "XA-TANTHOI"),
            ("Xã Tân Phú Đông",      "XA-TANPHUDONG"),

            // ── 20 PHƯỜNG ────────────────────────────────────
            // TP Mỹ Tho (83-87)
            ("Phường Mỹ Tho",         "PH-MYTHO"),
            ("Phường Đạo Thạnh",      "PH-DAOTHANH"),
            ("Phường Mỹ Phong",       "PH-MYPHONG"),
            ("Phường Thới Sơn",       "PH-THOISON"),
            ("Phường Trung An",       "PH-TRUNGAN"),
            // TP Gò Công (88-89)
            ("Phường Gò Công",         "PH-GOCONG"),
            ("Phường Long Chánh",     "PH-LONGCHANH"),
            // TP Cao Lãnh (90-93)
            ("Phường Cao Lãnh",        "PH-CAOLANH"),
            ("Phường Mỹ Ngãi",        "PH-MYNGAI"),
            ("Phường Hòa Thuận",      "PH-HOATHUAN"),
            ("Phường Mỹ Phú",         "PH-MYPHU"),
            // TX Hồng Ngự (94-95)
            ("Phường Hồng Ngự",        "PH-HONGNGU"),
            ("Phường An Lộc",          "PH-ANLOC"),
            // TP Sa Đéc (96-98)
            ("Phường Sa Đéc",          "PH-SADEC"),
            ("Phường Tân Quy",         "PH-TANQUY"),
            ("Phường An Hiệp",        "PH-ANHIEP"),
            // TX Cai Lậy (99-102)
            ("Phường Cai Lậy",         "PH-CAILAY"),
            ("Phường Nhị Quý",         "PH-NHIQUY"),
            ("Phường Tịnh Thới",       "PH-TINHTHOI"),
            ("Phường Long Hoa",        "PH-LONGHOA")
        };

        var departments = new List<Department>();
        foreach (var (name, code) in communeData)
        {
            departments.Add(new Department
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = $"UBND {name}",
                Code = code,
                Level = DepartmentLevel.Commune,
                ParentId = provinceId,
                ResponsibleUserId = null, // Sẽ gắn sau
                IsActive = true
            });
        }
        return departments;
    }

    // ══════════════════════════════════════════════════
    // ADMIN — Quản trị viên hệ thống
    // ══════════════════════════════════════════════════
    private static AppUser GetAdmin()
    {
        return new AppUser
        {
            Id = ObjectId.GenerateNewId().ToString(),
            FullName = "Quản trị viên Hệ thống",
            Email = "admin@dongthap.gov.vn",
            PasswordHash = DefaultPasswordHash,
            PhoneNumber = "0900000001",
            Role = UserRole.Admin,
            DepartmentId = null, // Admin không gắn đơn vị cụ thể
            IsActive = true,
            CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    // ══════════════════════════════════════════════════════
    // MANAGER — Lãnh đạo UBND tỉnh
    // ══════════════════════════════════════════════════════
    private static AppUser GetManager()
    {
        return new AppUser
        {
            Id = ObjectId.GenerateNewId().ToString(),
            FullName = "Phạm Thành Ngại",
            Email = "phamthanhngai@dongthap.gov.vn",
            PasswordHash = DefaultPasswordHash,
            PhoneNumber = "0901000001",
            Role = UserRole.Manager,
            DepartmentId = null, // Manager cấp tỉnh — không gắn department cụ thể
            IsActive = true,
            CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    // ══════════════════════════════════════════════════════
    // 3 DISPATCHER — Điều phối viên UBND tỉnh
    // ══════════════════════════════════════════════════════
    private static List<AppUser> GetDispatchers(string provinceDeptId)
    {
        return new List<AppUser>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Trần Thanh Tùng",
                Email = "tranthanhtung@dongthap.gov.vn",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0901100001",
                Role = UserRole.Dispatcher,
                DepartmentId = provinceDeptId,
                IsActive = true,
                CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Nguyễn Thị Mai Phương",
                Email = "ntmaiphuong@dongthap.gov.vn",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0901100002",
                Role = UserRole.Dispatcher,
                DepartmentId = provinceDeptId,
                IsActive = true,
                CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Lê Hoàng Dũng",
                Email = "lehoangdung@dongthap.gov.vn",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0901100003",
                Role = UserRole.Dispatcher,
                DepartmentId = provinceDeptId,
                IsActive = true,
                CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }

    // ══════════════════════════════════════════════════════
    // ASSIGNEE — Chủ tịch / Cán bộ xã phường
    // Tạo ít nhất 1 Assignee cho mỗi đơn vị xã/phường
    // + Thêm vài Assignee phụ cho các đơn vị lớn
    // ══════════════════════════════════════════════════════
    private static List<AppUser> GetAssignees(List<Department> communeDepts)
    {
        // Họ + Tên đệm + Tên phổ biến vùng ĐBSCL
        var lastNames = new[] {
            "Nguyễn", "Trần", "Lê", "Phạm", "Huỳnh",
            "Võ", "Đặng", "Bùi", "Hồ", "Dương",
            "Phan", "Lý", "Châu", "Tô", "Lâm"
        };
        var maleMiddleNames = new[] {
            "Văn", "Thanh", "Minh", "Hoàng", "Quốc",
            "Hữu", "Đình", "Ngọc", "Công", "Chí"
        };
        var femaleMiddleNames = new[] {
            "Thị", "Ngọc", "Thanh", "Bích", "Kim"
        };
        var maleFirstNames = new[] {
            "Hùng", "Dũng", "Tuấn", "Phong", "Bình",
            "Khải", "Tâm", "Thắng", "Trí", "Lực",
            "Nghĩa", "Đạt", "Phúc", "Vinh", "Tài",
            "Hải", "Long", "Sơn", "Kiệt", "Đức",
            "Nhật", "Thiện", "Toàn", "Cường", "An"
        };
        var femaleFirstNames = new[] {
            "Hương", "Linh", "Thảo", "Ngân", "Trang",
            "Yến", "Hạnh", "Oanh", "Diễm", "Trinh"
        };

        var assignees = new List<AppUser>();
        var rng = new Random(42); // Seed cố định để kết quả nhất quán
        int phoneCounter = 1;

        foreach (var dept in communeDepts)
        {
            // Mỗi xã/phường có 1 chủ tịch (Assignee chính)
            bool isFemale = rng.Next(100) < 25; // 25% nữ
            string fullName;
            if (isFemale)
            {
                fullName = $"{lastNames[rng.Next(lastNames.Length)]} " +
                    $"{femaleMiddleNames[rng.Next(femaleMiddleNames.Length)]} " +
                    $"{femaleFirstNames[rng.Next(femaleFirstNames.Length)]}";
            }
            else
            {
                fullName = $"{lastNames[rng.Next(lastNames.Length)]} " +
                    $"{maleMiddleNames[rng.Next(maleMiddleNames.Length)]} " +
                    $"{maleFirstNames[rng.Next(maleFirstNames.Length)]}";
            }

            var phone = $"09{phoneCounter:D8}";
            phoneCounter++;

            // Tạo email từ code đơn vị
            var emailCode = dept.Code.ToLower().Replace("-", "");
            assignees.Add(new AppUser
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = fullName,
                Email = $"ct.{emailCode}@dongthap.gov.vn",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = phone,
                Role = UserRole.Assignee,
                DepartmentId = dept.Id,
                IsActive = true,
                CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        return assignees;
    }

    // ══════════════════════════════════════════════════════
    // 5 CITIZEN — Người dân với điểm gamification hợp lý
    // ══════════════════════════════════════════════════════
    private static List<AppUser> GetCitizens()
    {
        var now = DateTime.UtcNow;
        return new List<AppUser>
        {
            // Citizen 1: Người dân tích cực nhất — gửi nhiều, ít bị từ chối
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Lê Thị Bích Ngọc",
                Email = "bichngoc.le87@gmail.com",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0388123456",
                Role = UserRole.Citizen,
                DepartmentId = null,
                IsActive = true,
                CreatedAt = new DateTime(2025, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                CitizenProfile = new CitizenProfile
                {
                    Address = "56 Nguyễn Huệ, Phường Mỹ Tho",
                    Ward = "Phường Mỹ Tho",
                    District = "TP. Mỹ Tho",
                    // Gửi 12 báo cáo, 10 duyệt, 1 từ chối, 1 đang xử lý
                    // 10 duyệt × 10đ = 100, 6 đóng × 5đ = 30, 1 rejected × -5đ = -5
                    // 3 rating × 2đ = 6, 1 first-of-month × 5đ = 5
                    // Tổng ≈ 136 điểm
                    TotalPoints = 136,
                    CurrentMonthPoints = 27,   // Tháng này gửi 2 báo cáo
                    CurrentQuarterPoints = 68,  // Quý này gửi 5 báo cáo
                    TotalTicketsSubmitted = 12,
                    TotalTicketsApproved = 10,
                    TotalTicketsRejected = 1,
                    LastActivityAt = now.AddDays(-2)
                }
            },
            // Citizen 2: Người dân bình thường — gửi vừa phải
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Trần Minh Hoàng",
                Email = "hoang.tranminh@gmail.com",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0377654321",
                Role = UserRole.Citizen,
                DepartmentId = null,
                IsActive = true,
                CreatedAt = new DateTime(2025, 9, 5, 0, 0, 0, DateTimeKind.Utc),
                CitizenProfile = new CitizenProfile
                {
                    Address = "128 đường 30/4, Phường Cao Lãnh",
                    Ward = "Phường Cao Lãnh",
                    District = "TP. Cao Lãnh",
                    // 6 báo cáo, 5 duyệt, 0 từ chối
                    // 5 × 10đ = 50, 3 đóng × 5đ = 15, 2 rating × 2đ = 4
                    // Tổng ≈ 69 điểm
                    TotalPoints = 69,
                    CurrentMonthPoints = 10,
                    CurrentQuarterPoints = 34,
                    TotalTicketsSubmitted = 6,
                    TotalTicketsApproved = 5,
                    TotalTicketsRejected = 0,
                    LastActivityAt = now.AddDays(-5)
                }
            },
            // Citizen 3: Người dân mới — vừa bắt đầu sử dụng
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Huỳnh Thanh Trúc",
                Email = "truchuynhthanh@gmail.com",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0359876543",
                Role = UserRole.Citizen,
                DepartmentId = null,
                IsActive = true,
                CreatedAt = now.AddDays(-20),
                CitizenProfile = new CitizenProfile
                {
                    Address = "Ấp 3, Xã Lai Vung",
                    Ward = "Xã Lai Vung",
                    District = "H. Lai Vung",
                    // 2 báo cáo, 2 duyệt, 0 từ chối
                    // 2 × 10đ = 20, 1 first-of-month × 5đ = 5
                    // Tổng = 25 điểm
                    TotalPoints = 25,
                    CurrentMonthPoints = 25,
                    CurrentQuarterPoints = 25,
                    TotalTicketsSubmitted = 2,
                    TotalTicketsApproved = 2,
                    TotalTicketsRejected = 0,
                    LastActivityAt = now.AddDays(-3)
                }
            },
            // Citizen 4: Người dân có báo cáo bị từ chối nhiều → điểm thấp
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Nguyễn Hữu Phát",
                Email = "phatnguyenhuu@gmail.com",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0912345678",
                Role = UserRole.Citizen,
                DepartmentId = null,
                IsActive = true,
                CreatedAt = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                CitizenProfile = new CitizenProfile
                {
                    Address = "Khóm 2, Phường Sa Đéc",
                    Ward = "Phường Sa Đéc",
                    District = "TP. Sa Đéc",
                    // 8 báo cáo, 4 duyệt, 3 từ chối, 1 đang xử lý
                    // 4 × 10đ = 40, 2 đóng × 5đ = 10, 3 rejected × -5đ = -15
                    // 1 rating × 2đ = 2
                    // Tổng = 37 điểm
                    TotalPoints = 37,
                    CurrentMonthPoints = 5,
                    CurrentQuarterPoints = 17,
                    TotalTicketsSubmitted = 8,
                    TotalTicketsApproved = 4,
                    TotalTicketsRejected = 3,
                    LastActivityAt = now.AddDays(-7)
                }
            },
            // Citizen 5: Người dân trung thành — hoạt động đều đặn
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Võ Thị Kim Loan",
                Email = "loankimvo@gmail.com",
                PasswordHash = DefaultPasswordHash,
                PhoneNumber = "0965432109",
                Role = UserRole.Citizen,
                DepartmentId = null,
                IsActive = true,
                CreatedAt = new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                CitizenProfile = new CitizenProfile
                {
                    Address = "Ấp Tân Lập, Xã Chợ Gạo",
                    Ward = "Xã Chợ Gạo",
                    District = "H. Chợ Gạo",
                    // 10 báo cáo, 9 duyệt, 0 từ chối
                    // 9 × 10đ = 90, 7 đóng × 5đ = 35, 4 rating × 2đ = 8
                    // 3 first-of-month × 5đ = 15, 2 consecutive × 3đ = 6
                    // Tổng = 154 điểm
                    TotalPoints = 154,
                    CurrentMonthPoints = 22,
                    CurrentQuarterPoints = 54,
                    TotalTicketsSubmitted = 10,
                    TotalTicketsApproved = 9,
                    TotalTicketsRejected = 0,
                    LastActivityAt = now.AddDays(-1)
                }
            }
        };
    }

    // ══════════════════════════════════════════════════════
    // GẮN ASSIGNEE VÀO ĐƠN VỊ (ResponsibleUserId)
    // ══════════════════════════════════════════════════════
    private async Task LinkAssigneesToDepartmentsAsync(
        List<Department> communeDepts, List<AppUser> assignees)
    {
        var deptCollection = _context.GetCollection<Department>();

        // Mỗi assignee được tạo theo thứ tự tương ứng với department
        for (int i = 0; i < communeDepts.Count && i < assignees.Count; i++)
        {
            var update = Builders<Department>.Update
                .Set(d => d.ResponsibleUserId, assignees[i].Id);
            await deptCollection.UpdateOneAsync(
                d => d.Id == communeDepts[i].Id, update);
        }
    }

    // ══════════════════════════════════════════════════════
    // SLA POLICIES — 1 chính sách / danh mục
    // ══════════════════════════════════════════════════════
    private static List<SlaPolicy> GetSlaPolicies(List<IncidentCategory> categories)
    {
        return categories.Select(c => new SlaPolicy
        {
            Id = ObjectId.GenerateNewId().ToString(),
            CategoryId = c.Id,
            ResolutionHours = c.DefaultSlaHours,
            WarningThresholdPercent = 80,
            ExcludeWeekends = true,
            ExcludeHolidays = true,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
    }

    // ══════════════════════════════════════════════════════
    // POINT POLICIES
    // ══════════════════════════════════════════════════════
    private static List<PointPolicy> GetPointPolicies()
    {
        return new List<PointPolicy>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Báo cáo được duyệt hợp lệ",
                Action = PointAction.TicketApproved, Points = 10,
                Description = "Cộng 10 điểm khi báo cáo của người dân được Dispatcher xác nhận hợp lệ",
                IsActive = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Báo cáo xử lý xong",
                Action = PointAction.TicketClosed, Points = 5,
                Description = "Cộng 5 điểm khi phản ánh được đóng thành công sau khi xử lý",
                IsActive = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Báo cáo bị từ chối",
                Action = PointAction.TicketRejected, Points = -5,
                Description = "Trừ 5 điểm khi báo cáo bị từ chối do spam hoặc không hợp lệ",
                IsActive = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Đánh giá kết quả xử lý",
                Action = PointAction.RatingSubmitted, Points = 2,
                Description = "Cộng 2 điểm khi người dân đánh giá kết quả xử lý phản ánh",
                IsActive = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Báo cáo đầu tiên trong tháng",
                Action = PointAction.FirstTicketOfMonth, Points = 5,
                Description = "Thưởng 5 điểm cho báo cáo đầu tiên mỗi tháng — khuyến khích tham gia",
                IsActive = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Name = "Hoạt động tích cực liên tiếp",
                Action = PointAction.ConsecutiveDaysActive, Points = 3,
                Description = "Thưởng 3 điểm khi người dân gửi báo cáo liên tiếp nhiều ngày",
                IsActive = true }
        };
    }

    // ══════════════════════════════════════════════════════
    // NGÀY LỄ 2026
    // ══════════════════════════════════════════════════════
    private static List<Holiday> GetHolidays()
    {
        return new List<Holiday>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 1, 1), Name = "Tết Dương lịch", IsRecurringYearly = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 14), Name = "Tết Nguyên đán — 28 Tết", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 15), Name = "Tết Nguyên đán — 29 Tết", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 16), Name = "Tết Nguyên đán — 30 Tết", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 17), Name = "Tết Nguyên đán — Mùng 1", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 18), Name = "Tết Nguyên đán — Mùng 2", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 19), Name = "Tết Nguyên đán — Mùng 3", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 2, 20), Name = "Tết Nguyên đán — Mùng 4", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 4, 6), Name = "Giỗ Tổ Hùng Vương (10/3 ÂL)", IsRecurringYearly = false },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 4, 30), Name = "Ngày Giải phóng miền Nam", IsRecurringYearly = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 5, 1), Name = "Ngày Quốc tế Lao động", IsRecurringYearly = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 9, 2), Name = "Ngày Quốc khánh", IsRecurringYearly = true },
            new() { Id = ObjectId.GenerateNewId().ToString(),
                Date = new DateTime(2026, 9, 3), Name = "Nghỉ bù Quốc khánh", IsRecurringYearly = false },
        };
    }

    // ══════════════════════════════════════════════════════
    // SEED TICKETS — 35 tickets đa dạng trạng thái
    // ══════════════════════════════════════════════════════
    private async Task SeedTicketsAsync(
        List<IncidentCategory> categories,
        List<Department> communeDepts,
        List<AppUser> dispatchers,
        List<AppUser> assignees,
        List<AppUser> citizens)
    {
        var ticketCol = _context.GetCollection<Ticket>();
        var activityCol = _context.GetCollection<TicketActivity>();
        var ratingCol = _context.GetCollection<Rating>();
        var pointHistoryCol = _context.GetCollection<PointHistory>();

        var now = DateTime.UtcNow;
        var rng = new Random(123);

        // Map category code → object để dễ lookup
        var catMap = categories.ToDictionary(c => c.Code);

        // ── Dữ liệu tickets — mỗi tuple mô tả 1 phản ánh thật ──
        var ticketData = new List<(
            string Title, string Description,
            string CategoryCode, TicketStatus Status,
            TicketPriority Priority,
            int CitizenIdx,    // Index trong mảng citizens
            int DeptIdx,       // Index trong mảng communeDepts
            int DaysAgo,       // Ngày tạo cách đây bao nhiêu ngày
            string Ward, string District,
            double Lon, double Lat, string Address,
            string? RejectionReason
        )>
        {
            // ── NEW (5 tickets) ──────────────────────────
            ("Cây xà cừ nghiêng đe dọa sập trên đường Lý Thường Kiệt",
             "Cây xà cừ lớn trước số 78 đường Lý Thường Kiệt bị nghiêng khoảng 30 độ sau trận mưa lớn đêm qua. Rễ cây đã bật khỏi mặt đất, rất nguy hiểm cho người đi đường.",
             "HT", TicketStatus.New, TicketPriority.Urgent,
             0, 82, 1, "Phường Mỹ Tho", "TP. Mỹ Tho",
             106.3542, 10.3540, "78 Lý Thường Kiệt, P. Mỹ Tho", null),

            ("Nước giếng khoan có mùi tanh bất thường ở ấp 5",
             "Khoảng 1 tuần nay, nước giếng khoan của nhiều hộ dân ấp 5 xã Tân Hồng có mùi tanh, để lắng có cặn vàng. Nghi ngờ nguồn nước bị nhiễm phèn từ khu đất đang san lấp gần đó.",
             "MT", TicketStatus.New, TicketPriority.High,
             1, 0, 2, "Xã Tân Hồng", "H. Tân Hồng",
             105.5735, 10.9108, "Ấp 5, Xã Tân Hồng", null),

            ("Đèn tín hiệu giao thông ngã tư Nguyễn Thái Học – Trần Hưng Đạo không hoạt động",
             "Đèn tín hiệu tại ngã tư Nguyễn Thái Học và Trần Hưng Đạo bị tắt hoàn toàn từ sáng nay, gây ùn tắc giờ cao điểm.",
             "GT", TicketStatus.New, TicketPriority.High,
             2, 87, 0, "Phường Cao Lãnh", "TP. Cao Lãnh",
             105.6270, 10.4608, "Ngã tư Nguyễn Thái Học – Trần Hưng Đạo, P. Cao Lãnh", null),

            ("Hàng quán lấn chiếm vỉa hè đường Nguyễn Sinh Sắc",
             "Nhiều quán cà phê và quán nhậu trên đường Nguyễn Sinh Sắc kê bàn ghế ra hết vỉa hè, người đi bộ phải đi xuống lòng đường rất nguy hiểm.",
             "ANTT", TicketStatus.New, TicketPriority.Normal,
             4, 87, 1, "Phường Cao Lãnh", "TP. Cao Lãnh",
             105.6312, 10.4570, "Nguyễn Sinh Sắc, P. Cao Lãnh", null),

            ("Trường tiểu học xã An Hữu thiếu nước sạch",
             "Hệ thống cấp nước của trường tiểu học An Hữu bị hư hỏng gần 1 tháng, học sinh phải mang nước từ nhà đi học.",
             "GD", TicketStatus.New, TicketPriority.High,
             0, 40, 3, "Xã An Hữu", "H. Cái Bè",
             106.0325, 10.3410, "Trường tiểu học An Hữu, Xã An Hữu", null),

            // ── UNDER REVIEW (4 tickets) ─────────────────
            ("Ổ gà lớn trên tuyến đường liên xã Tân Long – Thanh Bình",
             "Đoạn đường liên xã từ Tân Long đi Thanh Bình có nhiều ổ gà sâu khoảng 20cm, trải dài hơn 500m. Đã có 2 vụ tai nạn xe máy trong tuần qua.",
             "GT", TicketStatus.UnderReview, TicketPriority.High,
             1, 16, 5, "Xã Tân Long", "H. Thanh Bình",
             105.7812, 10.5940, "Đường liên xã Tân Long – Thanh Bình", null),

            ("Rác thải tập kết tự phát bên bờ kênh Nguyễn Văn Tiếp",
             "Bãi rác tự phát ngày càng lớn bên bờ kênh Nguyễn Văn Tiếp đoạn qua xã Tháp Mười, gây ô nhiễm nghiêm trọng, bốc mùi hôi thối đặc biệt vào buổi chiều.",
             "MT", TicketStatus.UnderReview, TicketPriority.Normal,
             4, 17, 4, "Xã Tháp Mười", "H. Tháp Mười",
             105.8850, 10.5256, "Bờ kênh Nguyễn Văn Tiếp, Xã Tháp Mười", null),

            ("Quán karaoke hoạt động xuyên đêm, gây ồn khu dân cư",
             "Quán karaoke Hoàng Gia trên đường Phạm Hữu Lầu mở cửa đến 2-3 giờ sáng hàng đêm, tiếng nhạc rất lớn, ảnh hưởng giấc ngủ của người dân xung quanh.",
             "ANTT", TicketStatus.UnderReview, TicketPriority.Normal,
             0, 94, 3, "Phường Sa Đéc", "TP. Sa Đéc",
             105.7580, 10.2926, "Phạm Hữu Lầu, P. Sa Đéc", null),

            ("Hệ thống thoát nước bị tắc nghẽn gây ngập cục bộ đường Trần Phú",
             "Mỗi khi mưa lớn, đoạn đường Trần Phú gần chợ Mỹ Tho bị ngập sâu 30-40cm do cống thoát nước tắc nghẽn, gây khó khăn cho người dân và tiểu thương.",
             "HT", TicketStatus.UnderReview, TicketPriority.High,
             2, 82, 6, "Phường Mỹ Tho", "TP. Mỹ Tho",
             106.3600, 10.3500, "Đường Trần Phú gần chợ Mỹ Tho", null),

            // ── ASSIGNED (4 tickets) ─────────────────────
            ("Mương thoát nước ấp 2 bị san lấp trái phép",
             "Một hộ dân tại ấp 2 xã Hòa Long đã san lấp đoạn mương thoát nước dài khoảng 50m để mở rộng đất. Nước không thoát được gây ngập ruộng lúa của các hộ xung quanh.",
             "NN", TicketStatus.Assigned, TicketPriority.High,
             1, 32, 8, "Xã Hòa Long", "H. Lai Vung",
             105.6790, 10.2255, "Ấp 2, Xã Hòa Long", null),

            ("Cơ sở kinh doanh thực phẩm không có giấy phép ATTP",
             "Quán cơm bình dân tại số 45 đường Lê Duẩn, phường Gò Công kinh doanh thức ăn nhưng không có giấy chứng nhận ATTP, nhà bếp không đảm bảo vệ sinh.",
             "YT", TicketStatus.Assigned, TicketPriority.Normal,
             3, 85, 7, "Phường Gò Công", "TX. Gò Công",
             106.6655, 10.3615, "45 Lê Duẩn, P. Gò Công", null),

            ("Đường dây điện trung thế võng thấp sát mái nhà dân",
             "Đường dây điện trung thế trên đường liên xã đoạn qua xã Phú Hựu bị võng thấp, gần chạm mái nhà dân. Khi có gió lớn, dây điện tỏa tia lửa rất nguy hiểm.",
             "DN", TicketStatus.Assigned, TicketPriority.Urgent,
             4, 35, 6, "Xã Phú Hựu", "H. Châu Thành",
             105.8720, 10.3150, "Đường liên xã, Xã Phú Hựu", null),

            ("Xây dựng nhà không phép trên đất nông nghiệp ấp Tân An",
             "Một hộ dân tại ấp Tân An, xã Kim Sơn đang xây nhà kiên cố trên đất nông nghiệp không có giấy phép xây dựng. Công trình đã xây được tầng trệt.",
             "XD", TicketStatus.Assigned, TicketPriority.Normal,
             0, 63, 9, "Xã Kim Sơn", "H. Châu Thành",
             106.3060, 10.3870, "Ấp Tân An, Xã Kim Sơn", null),

            // ── IN PROGRESS (5 tickets) ──────────────────
            ("Sửa chữa cầu gỗ liên xã bị mục sập một phần",
             "Cầu gỗ bắc qua kênh nối xã Tràm Chim với xã Phú Cường bị mục, sập một bên lan can và 2 tấm ván. Người dân phải đi đường vòng xa hơn 5km.",
             "GT", TicketStatus.InProgress, TicketPriority.Urgent,
             1, 10, 12, "Xã Tràm Chim", "H. Tam Nông",
             105.7232, 10.6945, "Cầu gỗ liên xã Tràm Chim – Phú Cường", null),

            ("Thu gom rác thải nhựa dọc bờ sông Tiền đoạn qua xã Thới Sơn",
             "Rác thải nhựa, túi nilon, chai lọ trôi dạt và mắc kẹt dày đặc dọc bờ sông Tiền đoạn qua phường Thới Sơn. Cần được thu gom khẩn cấp trước mùa nước nổi.",
             "MT", TicketStatus.InProgress, TicketPriority.Normal,
             0, 84, 10, "Phường Thới Sơn", "TP. Mỹ Tho",
             106.3720, 10.3380, "Bờ sông Tiền, P. Thới Sơn", null),

            ("Sửa chữa trạm bơm tưới tiêu ấp 3 xã Tân Nhuận Đông bị hỏng",
             "Trạm bơm ấp 3 bị hỏng motor từ 2 tuần nay, hơn 50ha lúa Đông Xuân không được tưới kịp thời. Nông dân thiệt hại nặng nếu không sửa gấp.",
             "NN", TicketStatus.InProgress, TicketPriority.Urgent,
             2, 36, 14, "Xã Tân Nhuận Đông", "H. Châu Thành",
             105.8250, 10.2630, "Ấp 3, Xã Tân Nhuận Đông", null),

            ("Thay thế đèn chiếu sáng đường liên ấp xã Mỹ Hiệp",
             "Tuyến đường liên ấp 1-2 xã Mỹ Hiệp dài 2km hoàn toàn không có đèn chiếu sáng ban đêm. Đã xảy ra nhiều vụ cướp giật vào buổi tối.",
             "HT", TicketStatus.InProgress, TicketPriority.High,
             4, 27, 11, "Xã Mỹ Hiệp", "H. Cao Lãnh",
             105.5840, 10.4020, "Đường liên ấp 1-2, Xã Mỹ Hiệp", null),

            ("Giải quyết tình trạng chó thả rông tại khu dân cư phường Mỹ Ngãi",
             "Khu dân cư phường Mỹ Ngãi có nhiều chó thả rông không rọ mõm, đã cắn 2 trẻ em trong tháng qua. Người dân rất lo lắng.",
             "YT", TicketStatus.InProgress, TicketPriority.High,
             3, 88, 9, "Phường Mỹ Ngãi", "TP. Cao Lãnh",
             105.6400, 10.4580, "Khu dân cư P. Mỹ Ngãi", null),

            // ── PENDING VERIFICATION (4 tickets) ─────────
            ("Vá ổ gà và tái trải nhựa đường Nguyễn Huệ",
             "Đường Nguyễn Huệ đoạn từ số 20 đến số 80 có hơn 10 ổ gà lớn, mặt đường bong tróc nghiêm trọng. Cần vá và tái trải nhựa gấp.",
             "GT", TicketStatus.PendingVerification, TicketPriority.Normal,
             0, 82, 20, "Phường Mỹ Tho", "TP. Mỹ Tho",
             106.3550, 10.3535, "Đường Nguyễn Huệ, P. Mỹ Tho", null),

            ("Dọn dẹp bãi rác tự phát và lắp biển cấm đổ rác",
             "Bãi rác tự phát tại góc đường Lê Lợi – Hai Bà Trưng đã tồn tại hơn 2 tháng, gây mất mỹ quan và ô nhiễm. Cần dọn dẹp và lắp biển cấm.",
             "MT", TicketStatus.PendingVerification, TicketPriority.Normal,
             1, 87, 18, "Phường Cao Lãnh", "TP. Cao Lãnh",
             105.6285, 10.4595, "Góc Lê Lợi – Hai Bà Trưng, P. Cao Lãnh", null),

            ("Sửa chữa hệ thống cấp nước sạch cho ấp 4 xã Lấp Vò",
             "Hệ thống cấp nước sạch ấp 4 bị vỡ ống chính, hơn 100 hộ dân mất nước sinh hoạt 3 ngày liền.",
             "DN", TicketStatus.PendingVerification, TicketPriority.Urgent,
             2, 30, 15, "Xã Lấp Vò", "H. Lấp Vò",
             105.6515, 10.2965, "Ấp 4, Xã Lấp Vò", null),

            ("Giải quyết khiếu nại về chậm cấp sổ đỏ tại UBND xã Châu Thành",
             "Gia đình tôi nộp hồ sơ xin cấp giấy chứng nhận QSDĐ từ tháng 3/2026, đến nay đã hơn 45 ngày mà chưa nhận được kết quả, cũng không ai thông báo lý do chậm trễ.",
             "HC", TicketStatus.PendingVerification, TicketPriority.Normal,
             3, 58, 16, "Xã Châu Thành", "H. Châu Thành",
             106.3415, 10.3700, "UBND Xã Châu Thành", null),

            // ── CLOSED (8 tickets) ───────────────────────
            ("Khắc phục đoạn đường sụt lún sau mưa tại xã Ba Sao",
             "Đoạn đường nội đồng tại ấp 2 xã Ba Sao bị sụt lún nghiêm trọng sau đợt mưa kéo dài, xe tải chở lúa không thể lưu thông.",
             "GT", TicketStatus.Closed, TicketPriority.High,
             0, 24, 35, "Xã Ba Sao", "H. Cao Lãnh",
             105.6730, 10.4810, "Ấp 2, Xã Ba Sao", null),

            ("Xử lý cơ sở chăn nuôi heo gây ô nhiễm ấp 1 xã Phong Hòa",
             "Trang trại chăn nuôi heo quy mô lớn xả nước thải chưa xử lý ra kênh, gây ô nhiễm nặng, cá chết hàng loạt, mùi hôi thối nồng nặc.",
             "MT", TicketStatus.Closed, TicketPriority.Urgent,
             1, 33, 40, "Xã Phong Hòa", "H. Lai Vung",
             105.6510, 10.2185, "Ấp 1, Xã Phong Hòa", null),

            ("Lắp đặt đèn chiếu sáng tuyến đường vào trường học xã Thanh Mỹ",
             "Con đường dẫn vào trường THCS Thanh Mỹ dài 500m không có đèn chiếu sáng, học sinh đi học sáng sớm và chiều tối rất nguy hiểm.",
             "HT", TicketStatus.Closed, TicketPriority.Normal,
             4, 18, 45, "Xã Thanh Mỹ", "H. Tháp Mười",
             105.8700, 10.5150, "Đường vào trường THCS Thanh Mỹ", null),

            ("Phản ánh cán bộ xã có thái độ thiếu tôn trọng công dân",
             "Khi đến UBND xã Vĩnh Kim làm thủ tục khai sinh cho con, tôi bị nhân viên tiếp dân có thái độ hách dịch, nói nặng lời và yêu cầu quay lại nhiều lần không rõ lý do.",
             "HC", TicketStatus.Closed, TicketPriority.Normal,
             0, 62, 50, "Xã Vĩnh Kim", "H. Châu Thành",
             106.3270, 10.3695, "UBND Xã Vĩnh Kim", null),

            ("Xử lý ổ dịch sốt xuất huyết tại ấp Phú Lợi xã Mỹ An Hưng",
             "Ấp Phú Lợi ghi nhận 5 ca sốt xuất huyết trong 2 tuần, có 1 ca nặng phải chuyển viện. Cần phun thuốc diệt muỗi khẩn cấp.",
             "YT", TicketStatus.Closed, TicketPriority.Urgent,
             2, 28, 30, "Xã Mỹ An Hưng", "H. Lấp Vò",
             105.6080, 10.3250, "Ấp Phú Lợi, Xã Mỹ An Hưng", null),

            ("Nạo vét kênh thủy lợi nội đồng bị bồi lắng tại xã Đốc Binh Kiều",
             "Kênh thủy lợi nội đồng ấp 3 bị bồi lắng nghiêm trọng, mực nước rất thấp, không đủ cung cấp cho hơn 30ha lúa vụ Đông Xuân.",
             "NN", TicketStatus.Closed, TicketPriority.High,
             4, 20, 28, "Xã Đốc Binh Kiều", "H. Tháp Mười",
             105.8350, 10.5560, "Ấp 3, Xã Đốc Binh Kiều", null),

            ("Sửa chữa mái tôn bị tốc ở nhà văn hóa ấp xã Tân Hộ Cơ",
             "Mái tôn nhà văn hóa ấp xã Tân Hộ Cơ bị gió lốc xoáy tốc mất một phần, bên trong bàn ghế và thiết bị bị hư hỏng do mưa tạt.",
             "VH", TicketStatus.Closed, TicketPriority.Normal,
             3, 2, 25, "Xã Tân Hộ Cơ", "H. Tân Hồng",
             105.5510, 10.8715, "Nhà văn hóa ấp, Xã Tân Hộ Cơ", null),

            ("Xử lý vi phạm xây dựng lấn chiếm hành lang kênh tại xã Phú Thọ",
             "Một hộ dân tại xã Phú Thọ xây nhà lấn chiếm hành lang bảo vệ kênh, thu hẹp dòng chảy gây ngập cục bộ cho các hộ lân cận.",
             "XD", TicketStatus.Closed, TicketPriority.High,
             1, 9, 38, "Xã Phú Thọ", "H. Tam Nông",
             105.7050, 10.6570, "Xã Phú Thọ, H. Tam Nông", null),

            // ── REJECTED (5 tickets) ─────────────────────
            ("Yêu cầu dọn bãi đậu xe trước nhà tôi",
             "Hàng xóm đậu xe trước nhà tôi, tôi yêu cầu chính quyền dọn đi.",
             "ANTT", TicketStatus.Rejected, TicketPriority.Low,
             3, 94, 22, "Phường Sa Đéc", "TP. Sa Đéc",
             105.7595, 10.2940, "Phường Sa Đéc", "Tranh chấp dân sự giữa các cá nhân, không thuộc thẩm quyền giải quyết của UBND. Đề nghị người dân tự thỏa thuận hoặc nhờ tổ hòa giải ấp."),

            ("Phản ánh giá xăng tăng quá cao",
             "Giá xăng bây giờ quá đắt, người dân không chịu nổi. Yêu cầu chính quyền tỉnh giảm giá xăng.",
             "KHAC", TicketStatus.Rejected, TicketPriority.Low,
             3, 87, 30, "Phường Cao Lãnh", "TP. Cao Lãnh",
             105.6300, 10.4610, "P. Cao Lãnh", "Giá xăng dầu do Chính phủ và Bộ Công Thương điều hành, không thuộc thẩm quyền cấp tỉnh."),

            ("Test hệ thống — vui lòng bỏ qua",
             "Đây là tin nhắn test, không phải phản ánh thật.",
             "KHAC", TicketStatus.Rejected, TicketPriority.Low,
             3, 82, 15, "Phường Mỹ Tho", "TP. Mỹ Tho",
             106.3540, 10.3530, "P. Mỹ Tho", "Nội dung không phải phản ánh hợp lệ (spam/test). Hệ thống chỉ tiếp nhận phản ánh, kiến nghị có nội dung thực tế."),

            ("Con đường trước nhà tôi bị bụi quá",
             "Xe tải chạy qua gây bụi.",
             "GT", TicketStatus.Rejected, TicketPriority.Low,
             3, 30, 20, "Xã Lấp Vò", "H. Lấp Vò",
             105.6520, 10.2970, "Xã Lấp Vò",
             "Nội dung quá chung chung, không rõ địa điểm cụ thể, thời gian xảy ra và mức độ ảnh hưởng. Vui lòng gửi lại với thông tin chi tiết hơn."),

            ("Phản ánh hàng xóm nuôi gà gây ồn",
             "Nhà hàng xóm nuôi mấy con gà trống gáy từ 4 giờ sáng làm tôi không ngủ được.",
             "ANTT", TicketStatus.Rejected, TicketPriority.Low,
             3, 58, 12, "Xã Châu Thành", "H. Châu Thành",
             106.3420, 10.3705, "Xã Châu Thành",
             "Hoạt động chăn nuôi gia cầm quy mô nhỏ trong khu dân cư nông thôn là phổ biến và không vi phạm quy định. Đề nghị người dân trao đổi trực tiếp với hàng xóm."),
        };

        var tickets = new List<Ticket>();
        var activities = new List<TicketActivity>();
        var ratings = new List<Rating>();
        var pointHistories = new List<PointHistory>();

        int ticketCounter = 1;

        foreach (var td in ticketData)
        {
            var citizen = citizens[td.CitizenIdx];
            var dept = communeDepts[td.DeptIdx];
            var category = catMap[td.CategoryCode];
            var createdAt = now.AddDays(-td.DaysAgo);
            var ticketCode = $"PA-{createdAt:MMyyyy}-{ticketCounter:D3}";
            ticketCounter++;

            // Chọn dispatcher và assignee phù hợp
            var dispatcher = dispatchers[rng.Next(dispatchers.Count)];
            // Assignee = chủ tịch xã/phường tương ứng
            var assignee = td.DeptIdx < assignees.Count
                ? assignees[td.DeptIdx] : assignees[0];

            var ticket = new Ticket
            {
                Id = ObjectId.GenerateNewId().ToString(),
                TicketCode = ticketCode,
                Title = td.Title,
                Description = td.Description,
                Status = td.Status,
                Priority = td.Priority,
                CategoryId = category.Id,
                Location = new GeoLocation
                {
                    Type = "Point",
                    Coordinates = new[] { td.Lon, td.Lat },
                    Address = td.Address
                },
                Attachments = new List<Attachment>(),
                ReporterId = citizen.Id,
                ReporterName = citizen.FullName,
                ReporterPhone = citizen.PhoneNumber,
                ReporterAddress = citizen.CitizenProfile?.Address,
                ReporterWard = td.Ward,
                ReporterDistrict = td.District,
                IsAccountCreated = true,
                SlaHours = category.DefaultSlaHours,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            // ── Thiết lập trạng thái xử lý theo workflow ──
            if (td.Status >= TicketStatus.UnderReview && td.Status != TicketStatus.New)
            {
                // Dispatcher đã duyệt
                ticket.ReviewedByDispatcherId = dispatcher.Id;
                ticket.ReviewedAt = createdAt.AddHours(rng.Next(2, 24));

                // Activity: Created
                activities.Add(NewActivity(ticket.Id, citizen.Id,
                    ActivityType.Created, null, TicketStatus.New,
                    "Người dân gửi phản ánh", createdAt));

                if (td.Status == TicketStatus.Rejected)
                {
                    ticket.RejectionReason = td.RejectionReason;
                    ticket.UpdatedAt = ticket.ReviewedAt!.Value;

                    activities.Add(NewActivity(ticket.Id, dispatcher.Id,
                        ActivityType.Rejected, TicketStatus.New, TicketStatus.Rejected,
                        td.RejectionReason, ticket.ReviewedAt!.Value));
                }
                else
                {
                    // Activity: Approved
                    activities.Add(NewActivity(ticket.Id, dispatcher.Id,
                        ActivityType.Approved, TicketStatus.New, TicketStatus.UnderReview,
                        "Phản ánh hợp lệ, tiến hành kiểm duyệt", ticket.ReviewedAt!.Value));
                }
            }

            if (td.Status >= TicketStatus.Assigned && td.Status != TicketStatus.Rejected)
            {
                // Giao việc cho đơn vị
                ticket.AssignedDepartmentId = dept.Id;
                ticket.AssignedUserId = assignee.Id;
                var assignedAt = ticket.ReviewedAt!.Value.AddHours(rng.Next(1, 8));
                ticket.UpdatedAt = assignedAt;

                activities.Add(NewActivity(ticket.Id, dispatcher.Id,
                    ActivityType.Assigned, TicketStatus.UnderReview, TicketStatus.Assigned,
                    $"Giao cho {dept.Name}", assignedAt));
            }

            if (td.Status >= TicketStatus.InProgress && td.Status != TicketStatus.Rejected)
            {
                // Assignee tiếp nhận xử lý
                var inProgressAt = ticket.UpdatedAt.AddHours(rng.Next(2, 24));
                ticket.SlaDeadline = inProgressAt.AddHours(category.DefaultSlaHours);
                ticket.UpdatedAt = inProgressAt;

                // Kiểm tra SLA breach cho ticket cũ
                if (ticket.SlaDeadline < now)
                    ticket.IsSlaBreached = true;

                activities.Add(NewActivity(ticket.Id, assignee.Id,
                    ActivityType.InProgress, TicketStatus.Assigned, TicketStatus.InProgress,
                    "Tiếp nhận và bắt đầu xử lý", inProgressAt));
            }

            if (td.Status >= TicketStatus.PendingVerification && td.Status != TicketStatus.Rejected)
            {
                // Assignee báo cáo kết quả
                var resultAt = ticket.UpdatedAt.AddDays(rng.Next(2, 7));
                ticket.UpdatedAt = resultAt;

                activities.Add(NewActivity(ticket.Id, assignee.Id,
                    ActivityType.ResultReported, TicketStatus.InProgress,
                    TicketStatus.PendingVerification,
                    "Đã hoàn thành xử lý, đính kèm ảnh kết quả", resultAt));
            }

            if (td.Status == TicketStatus.Closed)
            {
                // Dispatcher đóng ticket
                var closedAt = ticket.UpdatedAt.AddDays(rng.Next(1, 3));
                ticket.ClosedAt = closedAt;
                ticket.UpdatedAt = closedAt;
                // Closed tickets trước SLA = không breach
                ticket.IsSlaBreached = ticket.SlaDeadline.HasValue
                    && closedAt > ticket.SlaDeadline.Value;

                activities.Add(NewActivity(ticket.Id, dispatcher.Id,
                    ActivityType.Closed, TicketStatus.PendingVerification,
                    TicketStatus.Closed,
                    "Xác nhận kết quả xử lý, đóng phản ánh", closedAt));

                // Tạo Rating cho ~60% ticket Closed
                if (rng.Next(100) < 60)
                {
                    ratings.Add(new Rating
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        TicketId = ticket.Id,
                        CitizenId = citizen.Id,
                        Stars = rng.Next(3, 6), // 3-5 sao
                        Comment = GetRandomRatingComment(rng),
                        CreatedAt = closedAt.AddDays(rng.Next(1, 3))
                    });
                }
            }

            tickets.Add(ticket);
        }

        // ── Insert tất cả vào DB ──
        if (tickets.Count > 0)
            await ticketCol.InsertManyAsync(tickets);
        if (activities.Count > 0)
            await activityCol.InsertManyAsync(activities);
        if (ratings.Count > 0)
            await ratingCol.InsertManyAsync(ratings);

        Console.WriteLine($"  • {tickets.Count} phản ánh (tickets)");
        Console.WriteLine($"  • {activities.Count} hoạt động (activities)");
        Console.WriteLine($"  • {ratings.Count} đánh giá (ratings)");
    }

    // ── Helper: Tạo TicketActivity ───────────────────────
    private static TicketActivity NewActivity(
        string ticketId, string? actorId,
        ActivityType actionType,
        TicketStatus? oldStatus, TicketStatus? newStatus,
        string? comment, DateTime createdAt)
    {
        return new TicketActivity
        {
            Id = ObjectId.GenerateNewId().ToString(),
            TicketId = ticketId,
            ActorId = actorId,
            ActionType = actionType,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = comment,
            AttachmentUrls = new List<string>(),
            CreatedAt = createdAt
        };
    }

    // ── Helper: Random bình luận đánh giá ────────────────
    private static string GetRandomRatingComment(Random rng)
    {
        var comments = new[]
        {
            "Xử lý nhanh chóng, cảm ơn chính quyền!",
            "Đã giải quyết tốt, mong tiếp tục phát huy.",
            "Thời gian xử lý hơi lâu nhưng kết quả tốt.",
            "Rất hài lòng với cách giải quyết của cán bộ.",
            "Cần cải thiện thêm, chưa triệt để lắm.",
            "Cảm ơn, hy vọng vấn đề không tái diễn.",
            "Đánh giá 4 sao vì xử lý ổn nhưng phản hồi chậm.",
            "Tuyệt vời! Xử lý trong vòng 2 ngày.",
        };
        return comments[rng.Next(comments.Length)];
    }
}