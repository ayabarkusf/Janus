using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Janus.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? GradeLevel { get; set; }

    [StringLength(50)]
    public string? CareerInterest { get; set; }

    [StringLength(100)]
    public string? Company { get; set; }

    [StringLength(50)]
    public string? Industry { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    public bool IsStudent { get; set; }
    public bool IsHost { get; set; }
    public bool IsAdmin { get; set; }

    // Inactive users cannot log in or use protected actions.
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Opportunity> HostedOpportunities { get; set; } = new List<Opportunity>();
    public ICollection<OpportunityApplication> OpportunityApplications { get; set; } = new List<OpportunityApplication>();

    public string DisplayName => $"{FirstName} {LastName}".Trim();
}
