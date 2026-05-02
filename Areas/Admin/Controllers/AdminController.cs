using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppConstants.AdminRole)]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var current = await _userManager.GetUserAsync(User);
        if (current == null || !current.IsActive)
        {
            return Forbid();
        }

        var students = await _userManager.GetUsersInRoleAsync(AppConstants.StudentRole);
        var hosts = await _userManager.GetUsersInRoleAsync(AppConstants.HostRole);

        var model = new Janus.ViewModels.Admin.IndexModel(_userManager, _context)
        {
            TotalUsers = await _userManager.Users.CountAsync(u => u.IsActive),
            TotalStudents = students.Count(u => u.IsActive),
            TotalHosts = hosts.Count(u => u.IsActive),
            TotalApplications = await _context.OpportunityApplications.CountAsync(a => a.IsActive)
        };

        return View(model);
    }
}
