using Janus.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

public class InsightsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InsightsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new Janus.ViewModels.Insights.IndexModel(_context)
        {
            Insights = await _context.MarketInsights.OrderBy(i => i.Industry).ToListAsync()
        };

        return View(model);
    }
}
