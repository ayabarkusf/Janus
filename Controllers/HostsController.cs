using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

public class HostsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public HostsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hosts = await _userManager.GetUsersInRoleAsync(AppConstants.HostRole);

        var model = new Janus.ViewModels.Hosts.IndexModel(_userManager)
        {
            Hosts = hosts
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        var host = await _userManager.FindByIdAsync(id);
        if (host == null || !host.IsActive || !await _userManager.IsInRoleAsync(host, AppConstants.HostRole))
        {
            return NotFound();
        }

        var model = new Janus.ViewModels.Hosts.DetailsModel(_userManager, _context)
        {
            HostUser = host,
            Opportunities = await _context.Opportunities
                .Where(o => o.IsActive && o.IsOpen && o.HostUserId == id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync()
        };

        return View(model);
    }
}
