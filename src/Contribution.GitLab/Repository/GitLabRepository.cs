using System.Text.Json;
using Contribution.GitLab.Models;

namespace Contribution.GitLab.Repository;

public class GitLabRepository : IGitLabRepository
{
    private const string RestApiBase = "https://gitlab.com/api/v4";
    private readonly ILogger<GitLabRepository> _logger;
    private readonly HttpClient _client;

    public GitLabRepository(ILogger<GitLabRepository> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient("gitlab-rest");
        if (!_client.DefaultRequestHeaders.Contains("User-Agent"))
            _client.DefaultRequestHeaders.Add("User-Agent", "ContributionAPI");
    }

    public async Task<(List<GitLabEvent>? Events, string? Error)> GetUserContributionsAsync(
        string username, 
        DateTime from, 
        DateTime to, 
        string pat, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pat))
            return (null, "Missing PAT");

        var allEvents = new List<GitLabEvent>();
        var page = 1;
        const int perPage = 100; // Max allowed by GitLab API

        try
        {
            while (true)
            {
                // GitLab REST API: GET /users/:username/events
                var url = $"{RestApiBase}/users/{Uri.EscapeDataString(username)}/events?" +
                          $"after={Uri.EscapeDataString(from.ToString("yyyy-MM-dd"))}&" +
                          $"before={Uri.EscapeDataString(to.ToString("yyyy-MM-dd"))}&" +
                          $"per_page={perPage}&page={page}&sort=asc";

                var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
                
                // GitLab uses PRIVATE-TOKEN header for authentication
                httpReq.Headers.Add("PRIVATE-TOKEN", pat);

                var resp = await _client.SendAsync(httpReq, ct);
                
                if (!resp.IsSuccessStatusCode)
                {
                    var errorMsg = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}";
                    _logger.LogWarning("GitLab REST API call failed: {Error}", errorMsg);
                    return (null, errorMsg);
                }

                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var events = await JsonSerializer.DeserializeAsync<List<GitLabEvent>>(stream, cancellationToken: ct);
                
                if (events == null || events.Count == 0)
                    break;

                allEvents.AddRange(events);

                // Check if there are more pages
                if (events.Count < perPage)
                    break;

                page++;

                // Safety limit to avoid infinite loops
                if (page > 100) // Max 10,000 events
                {
                    _logger.LogWarning("Reached maximum page limit for user {Username}", username);
                    break;
                }
            }

            return (allEvents, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GitLab REST API");
            return (null, ex.Message);
        }
    }
}
