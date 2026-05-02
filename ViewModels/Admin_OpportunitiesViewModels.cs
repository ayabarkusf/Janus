using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Admin.Opportunities;
public class CreateModel
{
    public CreateModel() { }
    public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity Opportunity { get; set; } = new() { IsOpen = true, IsActive = true };
    public SelectList HostOptions { get; set; } = default!;
    public string[] Industries => AppConstants.Industries;
}

public class DeleteModel
{
    public DeleteModel() { }
    public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity? Opportunity { get; set; }
    public int OpportunityId { get; set; }
}

public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity? Opportunity { get; set; }
}

public class EditModel
{
    public EditModel() { }
    public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity Opportunity { get; set; } = new();
    public SelectList HostOptions { get; set; } = default!;
    public string[] Industries => AppConstants.Industries;
}

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public IList<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

