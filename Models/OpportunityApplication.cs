using System.ComponentModel.DataAnnotations;

namespace Janus.Models;

public class OpportunityApplication
{
    public int Id { get; set; }

    [Required]
    public string StudentUserId { get; set; } = string.Empty;

    public int OpportunityId { get; set; }

    [Required, RegularExpression("^(Pending|Accepted|Declined)$")]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string? CoverNote { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? StudentUser { get; set; }
    public Opportunity? Opportunity { get; set; }
}
