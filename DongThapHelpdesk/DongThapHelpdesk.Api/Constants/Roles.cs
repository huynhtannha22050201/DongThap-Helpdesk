namespace DongThapHelpdesk.Api.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Dispatcher = "Dispatcher";
    public const string Assignee = "Assignee";
    public const string Manager = "Manager";
    public const string Citizen = "Citizen";

    // Nhóm role dùng cho [Authorize]
    public const string AdminAndDispatcher =
        "Admin,Dispatcher";

    public const string AdminAndManager =
        "Admin,Manager";

    public const string StaffOnly =
        "Admin,Dispatcher,Assignee,Manager";

    public const string DispatcherAndManager =
        "Dispatcher,Manager,Admin";
}