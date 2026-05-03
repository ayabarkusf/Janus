using System.Text.Json.Serialization;

namespace Janus.Services;

public class CareerOneStopSettings
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.careeronestop.org/v1/";
}

public class SalaryResponse
{
    [JsonPropertyName("OccupationDetail")]
    public OccupationDetail? OccupationDetail { get; set; }
}

public class OccupationDetail
{
    [JsonPropertyName("OccupationTitle")]
    public string? OccupationTitle { get; set; }

    [JsonPropertyName("OccupationCode")]
    public string? OccupationCode { get; set; }

    [JsonPropertyName("SocInfo")]
    public List<SocInfo>? SocInfo { get; set; }

    [JsonPropertyName("Wages")]
    public WageData? Wages { get; set; }
}

public class SocInfo
{
    [JsonPropertyName("SocDescription")]
    public string? SocDescription { get; set; }
}

public class WageData
{
    [JsonPropertyName("NationalWagesList")]
    public List<WageEntry>? NationalWagesList { get; set; }
}

public class WageEntry
{
    [JsonPropertyName("RateType")]
    public string? RateType { get; set; }

    [JsonPropertyName("Median")]
    public string? Median { get; set; }

    [JsonPropertyName("Pct10")]
    public string? Pct10 { get; set; }

    [JsonPropertyName("Pct90")]
    public string? Pct90 { get; set; }
}

public class IndustryInsight
{
    public string Industry { get; set; } = string.Empty;
    public string OccupationTitle { get; set; } = string.Empty;
    public string OccupationCode { get; set; } = string.Empty;
    public string? MedianAnnualWage { get; set; }
    public string? MedianHourlyWage { get; set; }
    public bool BrightOutlook { get; set; }
    public string? EducationLevel { get; set; }
    public string? Description { get; set; }
}