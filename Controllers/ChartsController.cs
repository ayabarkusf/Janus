using Janus.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Janus.Controllers;

public class ChartsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChartsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new Janus.ViewModels.Charts.IndexModel(_context);

        var industry = await _context.Opportunities.Where(o => o.IsActive)
            .GroupBy(o => o.Industry)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderBy(x => x.Label)
            .ToListAsync();

        var status = await (
            from a in _context.OpportunityApplications
            where a.IsActive
            join o in _context.Opportunities on a.OpportunityId equals o.Id
            group a by o.Industry into g
            orderby g.Key
            select new { Label = g.Key, Count = g.Count() }
        ).ToListAsync();

        var months = await _context.OpportunityApplications.Where(a => a.IsActive)
            .GroupBy(a => new { a.AppliedAt.Year, a.AppliedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var city = await _context.Opportunities.Where(o => o.IsActive)
            .GroupBy(o => o.City)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderBy(x => x.Label)
            .ToListAsync();

        model.IndustryLabelsJson = JsonSerializer.Serialize(industry.Select(x => x.Label));
        model.IndustryCountsJson = JsonSerializer.Serialize(industry.Select(x => x.Count));
        model.StatusLabelsJson = JsonSerializer.Serialize(status.Select(x => x.Label));
        model.StatusCountsJson = JsonSerializer.Serialize(status.Select(x => x.Count));
        model.MonthLabelsJson = JsonSerializer.Serialize(months.Select(x => $"{x.Month:00}/{x.Year}"));
        model.MonthCountsJson = JsonSerializer.Serialize(months.Select(x => x.Count));
        model.CityLabelsJson = JsonSerializer.Serialize(city.Select(x => x.Label));
        model.CityCountsJson = JsonSerializer.Serialize(city.Select(x => x.Count));

        return View(model);
    }
}
