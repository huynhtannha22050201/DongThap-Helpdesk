namespace DongThapHelpdesk.Api.Services;

public class SmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendAccountCreatedAsync(
        string phoneNumber,
        string password)
    {
        // Nội dung SMS gửi cho người dân
        var message =
            $"Chao mung ban den voi He thong Phan anh " +
            $"Dong Thap!\n" +
            $"Tai khoan cua ban:\n" +
            $"- So dien thoai: {phoneNumber}\n" +
            $"- Mat khau: {password}\n" +
            $"Vui long dang nhap tai: " +
            $"http://helpdesk.dongthap.gov.vn";

        // TODO: Tích hợp SMS gateway thật
        // Ví dụ: VNPT SMS, Viettel SMS, Twilio...
        // await _smsGateway.SendAsync(phoneNumber, message);

        // Hiện tại chỉ log ra console khi dev
        _logger.LogInformation(
            "📱 SMS gửi đến {Phone}: {Message}",
            phoneNumber, message);

        await Task.CompletedTask;
    }

    public async Task SendTicketApprovedAsync(
        string phoneNumber,
        string ticketCode)
    {
        var message =
            $"Phan anh {ticketCode} cua ban da duoc " +
            $"tiep nhan va dang xu ly.\n" +
            $"Theo doi tai: " +
            $"http://helpdesk.dongthap.gov.vn";

        _logger.LogInformation(
            "📱 SMS gửi đến {Phone}: {Message}",
            phoneNumber, message);

        await Task.CompletedTask;
    }

    public async Task SendTicketClosedAsync(
        string phoneNumber,
        string ticketCode)
    {
        var message =
            $"Phan anh {ticketCode} da duoc xu ly xong.\n" +
            $"Moi ban danh gia chat luong xu ly tai: " +
            $"http://helpdesk.dongthap.gov.vn";

        _logger.LogInformation(
            "📱 SMS gửi đến {Phone}: {Message}",
            phoneNumber, message);

        await Task.CompletedTask;
    }
}