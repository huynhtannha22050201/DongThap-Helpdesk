namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class RatingRequest
{
    public int Stars { get; set; }
    // Số sao đánh giá từ 1-5

    public string? Comment { get; set; }
    // Nhận xét của người dân
}