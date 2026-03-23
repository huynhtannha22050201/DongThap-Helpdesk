using DongThapHelpdesk.Api.Configurations;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Services;

public class FileUploadService
{
    private readonly FileUploadSettings _settings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(
        FileUploadSettings settings,
        IWebHostEnvironment env,
        ILogger<FileUploadService> logger)
    {
        _settings = settings;
        _env = env;
        _logger = logger;
    }

    public async Task<(bool Success,
        string Message,
        List<Attachment> Attachments)>
        UploadFilesAsync(
        List<IFormFile> files,
        string subFolder = "tickets")
    {
        var attachments = new List<Attachment>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var (success, message, attachment) =
                await UploadSingleFileAsync(file, subFolder);

            if (success && attachment != null)
                attachments.Add(attachment);
            else
                errors.Add($"{file.FileName}: {message}");
        }

        if (errors.Any())
            return (false,
                string.Join(", ", errors),
                attachments);

        return (true, "Upload thành công", attachments);
    }

    public async Task<(bool Success,
        string Message,
        Attachment? Attachment)>
        UploadSingleFileAsync(
        IFormFile file,
        string subFolder)
    {
        // Nghiệp vụ: validate kích thước
        if (file.Length > _settings.MaxFileSizeBytes)
            return (false,
                $"File vượt quá {_settings.MaxFileSizeMb}MB",
                null);

        // Nghiệp vụ: validate đuôi file
        var ext = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!_settings.AllowedExtensions.Contains(ext))
            return (false,
                $"Định dạng {ext} không được hỗ trợ" +
                $"Chỉ chấp nhận: " +
                $"{string.Join(", ", _settings.AllowedExtensions)}",
                null);

        // Nghiệp vụ: tạo tên file unique
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";

        // Nghiệp vụ: tạo đường dẫn lưu file
        var uploadFolder = Path.Combine(
            _env.ContentRootPath,
            _settings.UploadPath,
            subFolder,
            DateTime.UtcNow.ToString("yyyy/MM"));
        // Tổ chức theo năm/tháng
        // Ví dụ: wwwroot/uploads/tickets/2026/03/

        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        var filePath = Path.Combine(
            uploadFolder, uniqueFileName);

        // Lưu file xuống disk
        using (var stream = new FileStream(
            filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Tạo URL truy cập public
        var relativePath = Path.Combine(
            _settings.UploadPath,
            subFolder,
            DateTime.UtcNow.ToString("yyyy/MM"),
            uniqueFileName)
            .Replace("\\", "/");

        var attachment = new Attachment
        {
            FileName = file.FileName,
            Url = $"/{relativePath}",
            MimeType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "✓ Upload thành công: {FileName} → {Url}",
            file.FileName, attachment.Url);

        return (true, "Upload thành công", attachment);
    }

    public bool DeleteFile(string fileUrl)
    {
        try
        {
            var filePath = Path.Combine(
                _env.WebRootPath,
                fileUrl.TrimStart('/'));

            if (!File.Exists(filePath)) return false;

            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Lỗi xóa file: {Error}", ex.Message);
            return false;
        }
    }
}