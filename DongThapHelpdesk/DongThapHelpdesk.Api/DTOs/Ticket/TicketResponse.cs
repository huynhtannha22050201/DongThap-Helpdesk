namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class TicketResponse
{
    public string Id { get; set; } = null!;
    public string TicketCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;

    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    // Tên danh mục để hiển thị trên UI

    public LocationResponse? Location { get; set; }
    public List<AttachmentResponse> Attachments { get; set; }
        = new();

    // Thông tin người báo cáo
    public string ReporterName { get; set; } = null!;
    public string ReporterPhone { get; set; } = null!;
    public string ReporterAddress { get; set; }
    public string? ReporterWard { get; set; }
    public string? ReporterDistrict { get; set; }

    // Thông tin xử lý
    public string? AssignedDepartmentId { get; set; }
    public string? AssignedDepartmentName { get; set; }
    public string? AssignedUserName { get; set; }

    // Thông tin SLA
    public int SlaHours { get; set; }
    public DateTime? SlaDeadline { get; set; }
    public bool IsSlaBreached { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class LocationResponse
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? Address { get; set; }
}

public class AttachmentResponse
{
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}