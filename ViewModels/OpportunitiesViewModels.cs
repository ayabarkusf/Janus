using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Opportunities;
public class CreateModel
{
    public CreateModel() { }
    public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity Opportunity { get; set; } = new();
    public string[] Industries => AppConstants.Industries;
    public SelectList HostOptions { get; set; } = default!;
    public bool CanSelectHost { get; set; }
}

public class DeleteModel
{
    public DeleteModel() { }
    public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity? Opportunity { get; set; }
}

public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(ApplicationDbContext context) { }
    public Opportunity? Opportunity { get; set; }
}

public class EditModel
{
    public EditModel() { }
    public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity Opportunity { get; set; } = new();
    public string[] Industries => AppConstants.Industries;
}

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context) { }
    public IList<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public string[] Industries
    {
        get
        {
            var result = new List<string>();
            foreach (var industry in AppConstants.Industries)
            {
                if (industry != "Other")
                    result.Add(industry);
            }
            return result.ToArray();
        }
    }
    public string? Industry { get; set; }
    public string? City { get; set; }
}

public class MyOpportunitiesModel
{
    public MyOpportunitiesModel() { }
    public MyOpportunitiesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public IList<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

