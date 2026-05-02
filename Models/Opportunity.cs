using System.ComponentModel.DataAnnotations;

namespace Janus.Models;

public class Opportunity
{
    public int Id { get; set; }

    [Required]
    public string HostUserId { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Industry { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string SkillsTaught { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsOpen { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? HostUser { get; set; }
    public ICollection<OpportunityApplication> Applications { get; set; } = new List<OpportunityApplication>();
}
