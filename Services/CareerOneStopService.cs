using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Janus.Services;

public class CareerOneStopService
{
    private readonly HttpClient _http;
    private readonly CareerOneStopSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CareerOneStopService> _logger;

    private static readonly Dictionary<string, (string Code, string Title)> KeywordMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["nurse"] = ("29-1141.00", "Registered Nurses"),
            ["registered nurse"] = ("29-1141.00", "Registered Nurses"),
            ["rn"] = ("29-1141.00", "Registered Nurses"),
            ["doctor"] = ("29-1221.00", "Physicians and Surgeons"),
            ["physician"] = ("29-1221.00", "Physicians and Surgeons"),
            ["dentist"] = ("29-1021.00", "Dentists"),
            ["pharmacist"] = ("29-1051.00", "Pharmacists"),
            ["physical therapist"] = ("29-1123.00", "Physical Therapists"),
            ["healthcare"] = ("29-1141.00", "Registered Nurses"),
            ["software developer"] = ("15-1252.00", "Software Developers"),
            ["software engineer"] = ("15-1252.00", "Software Developers"),
            ["programmer"] = ("15-1252.00", "Software Developers"),
            ["developer"] = ("15-1252.00", "Software Developers"),
            ["data scientist"] = ("15-2051.00", "Data Scientists"),
            ["data analyst"] = ("15-2051.00", "Data Scientists"),
            ["cybersecurity"] = ("15-1212.00", "Information Security Analysts"),
            ["security analyst"] = ("15-1212.00", "Information Security Analysts"),
            ["technology"] = ("15-1252.00", "Software Developers"),
            ["financial analyst"] = ("13-2051.00", "Financial Analysts"),
            ["accountant"] = ("13-2011.00", "Accountants and Auditors"),
            ["auditor"] = ("13-2011.00", "Accountants and Auditors"),
            ["finance"] = ("13-2051.00", "Financial Analysts"),
            ["financial advisor"] = ("13-2052.00", "Personal Financial Advisors"),
            ["economist"] = ("19-3011.00", "Economists"),
            ["lawyer"] = ("23-1011.00", "Lawyers"),
            ["attorney"] = ("23-1011.00", "Lawyers"),
            ["law"] = ("23-1011.00", "Lawyers"),
            ["paralegal"] = ("23-2011.00", "Paralegals and Legal Assistants"),
            ["judge"] = ("23-1023.00", "Judges and Hearing Officers"),
            ["teacher"] = ("25-2021.00", "Elementary School Teachers"),
            ["professor"] = ("25-1099.00", "Postsecondary Teachers"),
            ["education"] = ("25-2021.00", "Elementary School Teachers"),
            ["school counselor"] = ("21-1012.00", "Educational Counselors"),
            ["principal"] = ("11-9032.00", "Education Administrators"),
        };

    public CareerOneStopService(
        HttpClient http,
        IOptions<CareerOneStopSettings> settings,
        IMemoryCache cache,
        ILogger<CareerOneStopService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<IndustryInsight>> SearchOccupationsAsync(string keyword)
    {
        var trimmed = keyword.Trim();
        var cacheKey = $"search_{trimmed.ToLower()}";

        if (_cache.TryGetValue(cacheKey, out List<IndustryInsight>? cached) && cached is not null)
            return cached;

        var lookup = FindBestMatch(trimmed);
        var apiQuery = lookup.HasValue ? lookup.Value.Code : trimmed;
        var displayTitle = lookup.HasValue ? lookup.Value.Title : trimmed;

        var results = await FetchSalaryAsync(apiQuery, displayTitle);

        _cache.Set(cacheKey, results, TimeSpan.FromHours(6));
        return results;
    }

    private (string Code, string Title)? FindBestMatch(string input)
    {
        if (KeywordMap.TryGetValue(input, out var exact))
            return exact;

        foreach (var kvp in KeywordMap)
        {
            if (input.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains(input, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return null;
    }

    private async Task<List<IndustryInsight>> FetchSalaryAsync(string query, string displayTitle)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"https://api.careeronestop.org/v1/comparesalaries/{_settings.UserId}/wage"
                + $"?keyword={encodedQuery}&location=us&enableMetaData=0";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.Token);

        using var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("CareerOneStop [{Status}] query='{Query}'",
            response.StatusCode, query);

        if (!response.IsSuccessStatusCode)
            return new List<IndustryInsight>();

        var envelope = JsonSerializer.Deserialize<SalaryResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var detail = envelope?.OccupationDetail;
        if (detail is null)
            return new List<IndustryInsight>();

        var annualWage = detail.Wages?.NationalWagesList?.FirstOrDefault(w =>
            string.Equals(w.RateType, "Annual", StringComparison.OrdinalIgnoreCase));
        var hourlyWage = detail.Wages?.NationalWagesList?.FirstOrDefault(w =>
            string.Equals(w.RateType, "Hourly", StringComparison.OrdinalIgnoreCase));

        var description = detail.SocInfo?.FirstOrDefault()?.SocDescription;

        return new List<IndustryInsight>
        {
            new IndustryInsight
            {
                OccupationTitle  = detail.OccupationTitle ?? displayTitle,
                OccupationCode   = detail.OccupationCode  ?? string.Empty,
                Description      = TruncateDescription(description, 2000),
                MedianAnnualWage = FormatWage(annualWage?.Median, "$", "/yr"),
                MedianHourlyWage = FormatWage(hourlyWage?.Median, "$", "/hr"),
            }
        };
    }

    private static string? FormatWage(string? raw, string prefix, string suffix)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (!decimal.TryParse(raw, out var value)) return raw;
        return suffix == "/yr"
            ? $"{prefix}{value:N0}{suffix}"
            : $"{prefix}{value:F2}{suffix}";
    }

    private static string? TruncateDescription(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "…";
    }
}