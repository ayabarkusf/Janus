using System.ComponentModel.DataAnnotations;

namespace Janus.Models;

public class MarketInsight
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Industry { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string MetricName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string MetricValue { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SourceName { get; set; }

    [StringLength(300)]
    public string? SourceUrl { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
