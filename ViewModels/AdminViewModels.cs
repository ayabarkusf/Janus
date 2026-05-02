using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;

namespace Janus.ViewModels.Admin;

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context) { }
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalHosts { get; set; }
    public int TotalApplications { get; set; }
}
