using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Charts;
public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context) { }
    public string IndustryLabelsJson { get; set; } = "[]";
    public string IndustryCountsJson { get; set; } = "[]";
    public string StatusLabelsJson { get; set; } = "[]";
    public string StatusCountsJson { get; set; } = "[]";
    public string MonthLabelsJson { get; set; } = "[]";
    public string MonthCountsJson { get; set; } = "[]";
    public string CityLabelsJson { get; set; } = "[]";
    public string CityCountsJson { get; set; } = "[]";
}

