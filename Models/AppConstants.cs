namespace Janus.Models;

public static class AppConstants
{
    public const string StudentRole = "Student";
    public const string HostRole = "Host";
    public const string AdminRole = "Admin";

    public static readonly string[] Roles =
    {
        StudentRole, HostRole, AdminRole
    };

    public static readonly string[] Industries =
    {
        "Healthcare", "Technology", "Finance", "Law", "Education", "Other"
    };

    public static readonly string[] GradeLevels =
    {
        "9th", "10th", "11th", "12th"
    };

    public static readonly string[] ApplicationStatuses =
    {
        "Pending", "Accepted", "Declined"
    };
}
