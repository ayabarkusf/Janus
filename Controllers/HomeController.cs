using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new Janus.ViewModels.IndexModel(_context)
        {
            FeaturedOpportunities = await _context.Opportunities
                .Where(o => o.IsActive && o.IsOpen && o.HostUser != null && o.HostUser.IsActive)
                .Include(o => o.HostUser)
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult About() => View(new Janus.ViewModels.AboutModel());
    public IActionResult Privacy() => View(new Janus.ViewModels.PrivacyModel());
    public IActionResult Bot() => View(new Janus.ViewModels.BotModel());
    public IActionResult AccessDenied() => View(new Janus.ViewModels.AccessDeniedModel());
    public IActionResult Error() => View(new Janus.ViewModels.ErrorModel());
}
