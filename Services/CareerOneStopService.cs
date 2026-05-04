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

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

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

        var results = await FetchOccupationsAsync(trimmed);
        _cache.Set(cacheKey, results, TimeSpan.FromHours(6));
        return results;
    }

    private async Task<List<IndustryInsight>> FetchOccupationsAsync(string keyword)
    {
        // Step 1: occupation search — returns titles, codes, descriptions
        var encodedKeyword = Uri.EscapeDataString(keyword);
        var searchUrl = $"https://api.careeronestop.org/v1/occupation/{_settings.UserId}/{encodedKeyword}/us/0/5";

        using var searchRequest = new HttpRequestMessage(HttpMethod.Get, searchUrl);
        searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);

        using var searchResponse = await _http.SendAsync(searchRequest);
        var searchJson = await searchResponse.Content.ReadAsStringAsync();

        _logger.LogInformation("CareerOneStop occupation search [{Status}] keyword='{Keyword}'",
            searchResponse.StatusCode, keyword);

        if (!searchResponse.IsSuccessStatusCode)
            return new List<IndustryInsight>();

        var envelope = JsonSerializer.Deserialize<OccupationSearchResponse>(searchJson, JsonOpts);

        if (envelope?.OccupationList is null || envelope.OccupationList.Count == 0)
            return new List<IndustryInsight>();

        // Step 2: fetch wages for each occupation in parallel
        var wageTasks = envelope.OccupationList
            .Select(occ => FetchWagesAsync(occ.OnetCode ?? string.Empty))
            .ToList();

        var wageResults = await Task.WhenAll(wageTasks);

        // Step 3: combine
        var results = new List<IndustryInsight>();
        for (int i = 0; i < envelope.OccupationList.Count; i++)
        {
            var occ = envelope.OccupationList[i];
            var wages = wageResults[i];

            var annual = wages?.FirstOrDefault(w =>
                string.Equals(w.RateType, "Annual", StringComparison.OrdinalIgnoreCase));
            var hourly = wages?.FirstOrDefault(w =>
                string.Equals(w.RateType, "Hourly", StringComparison.OrdinalIgnoreCase));

            results.Add(new IndustryInsight
            {
                OccupationTitle  = occ.OnetTitle  ?? string.Empty,
                OccupationCode   = occ.OnetCode   ?? string.Empty,
                Description      = TruncateDescription(occ.OccupationDescription, 2000),
                MedianAnnualWage = FormatWage(annual?.Median, "$", "/yr"),
                MedianHourlyWage = FormatWage(hourly?.Median, "$", "/hr"),
                BrightOutlook    = occ.BrightOutlook,
            });
        }

        return results;
    }

    private async Task<List<WageEntry>?> FetchWagesAsync(string onetCode)
    {
        if (string.IsNullOrWhiteSpace(onetCode)) return null;

        var encoded = Uri.EscapeDataString(onetCode);
        var url = $"https://api.careeronestop.org/v1/comparesalaries/{_settings.UserId}/wage"
                + $"?keyword={encoded}&location=us&enableMetaData=0";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);

            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<SalaryResponse>(json, JsonOpts);
            return envelope?.OccupationDetail?.Wages?.NationalWagesList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Wage fetch failed for code='{Code}'", onetCode);
            return null;
        }
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
