using Janus.Data;
using Janus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

public class InsightsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CareerOneStopService _careerService;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(
        ApplicationDbContext context,
        CareerOneStopService careerService,
        ILogger<InsightsController> logger)
    {
        _context = context;
        _careerService = careerService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? keyword)
    {
        var model = new Janus.ViewModels.Insights.IndexModel(_context)
        {
            Keyword = keyword ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            try
            {
                model.SearchResults = await _careerService.SearchOccupationsAsync(keyword);
                model.SearchPerformed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CareerOneStop search failed for keyword={Keyword}", keyword);
                model.SearchError = "Search is temporarily unavailable. Please try again later.";
                model.SearchPerformed = true;
            }
        }

        return View(model);
    }
}