using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Hosts;
public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context) { }
    public ApplicationUser? HostUser { get; set; }
    public IList<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(UserManager<ApplicationUser> userManager) { }
    public IList<ApplicationUser> Hosts { get; set; } = new List<ApplicationUser>();
}

