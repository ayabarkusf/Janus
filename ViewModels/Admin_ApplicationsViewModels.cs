using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Admin.Applications;
public class DeleteModel
{
    public DeleteModel() { }
    public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public OpportunityApplication? Application { get; set; }
    public int ApplicationId { get; set; }
}

public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public OpportunityApplication? Application { get; set; }
}

public class EditModel
{
    public EditModel() { }
    public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public InputModel Input { get; set; } = new();
    public string StudentName { get; set; } = string.Empty;
    public string OpportunityTitle { get; set; } = string.Empty;
    public string[] Statuses => AppConstants.ApplicationStatuses;

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? CoverNote { get; set; }

        public bool IsActive { get; set; }
    }
}

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public IList<OpportunityApplication> Applications { get; set; } = new List<OpportunityApplication>();
}

