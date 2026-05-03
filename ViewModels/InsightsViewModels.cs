using Janus.Data;
using Janus.Models;
using Janus.Services;

namespace Janus.ViewModels.Insights;

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context) { }

    // Search inputs
    public string Keyword { get; set; } = string.Empty;

    // Search state
    public bool SearchPerformed { get; set; }
    public string? SearchError { get; set; }

    // Search results from CareerOneStop API
    public IList<IndustryInsight> SearchResults { get; set; } = new List<IndustryInsight>();

    // Fallback seeded database records
    public IList<MarketInsight> Insights { get; set; } = new List<MarketInsight>();
}