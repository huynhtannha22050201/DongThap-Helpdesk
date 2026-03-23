namespace DongThapHelpdesk.Api.Configurations
{
    public class FileUploadSettings
    {
        public string UploadPath { get; set; } = null!;
        // Thư mục lưu file trên server
        // Ví dụ: "wwwroot/uploads"

        public int MaxFileSizeMb { get; set; } = 10;
        // Kích thước file tối đa tính bằng MB

        public List<string> AllowedExtensions { get; set; }
            = new();
        // Danh sách đuôi file được phép
        // Ví dụ: [".jpg", ".png", ".mp4"]

        public long MaxFileSizeBytes
            => MaxFileSizeMb * 1024 * 1024;
        // Chuyển MB sang Bytes để so sánh
    }
}
