using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Applications;
public class ApplyModel
{
    public ApplyModel() { }
    public ApplyModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public Opportunity? Opportunity { get; set; }
    public bool NeedsStudentProfile { get; set; }
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int OpportunityId { get; set; }

        [StringLength(500)]
        [Display(Name = "Why are you interested in this opportunity?")]
        public string? CoverNote { get; set; }
    }
}

public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public OpportunityApplication? Application { get; set; }
    public bool CanEditStatus { get; set; }
    public string[] Statuses => AppConstants.ApplicationStatuses;
    public StatusInputModel StatusInput { get; set; } = new();

    public class StatusInputModel
    {
        public int ApplicationId { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}

public class MyApplicationsModel
{
    public MyApplicationsModel() { }
    public MyApplicationsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { }
    public IList<OpportunityApplication> Applications { get; set; } = new List<OpportunityApplication>();
}

public class ThankYouModel
{
    public ThankYouModel() { }
    public ThankYouModel(ApplicationDbContext context) { }
    public string OpportunityTitle { get; set; } = "this opportunity";
}

