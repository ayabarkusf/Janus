using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Insights;
public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context) { }
    public IList<MarketInsight> Insights { get; set; } = new List<MarketInsight>();
}

