using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Contribution.GitHub.Models;

namespace Contribution.GitHub.Repository;

public class GitHubRepository(ILogger<GitHubRepository> logger, IHttpClientFactory httpClientFactory) : IGitHubRepository
{
    private const string GraphQlEndpoint = "https://api.github.com/graphql";
    private readonly ILogger<GitHubRepository> _logger = logger;
    private readonly HttpClient _client = httpClientFactory.CreateClient("github-graphql");

    public async Task<(GitHubUser? User, List<GitHubGraphQLError>? Errors)> GetUserContributionsAsync(string username, DateTime from, DateTime to, string pat, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pat))
            return (null, [new GitHubGraphQLError { Message = "Missing PAT" }]);

        // Build GraphQL query payload
        var query = @"
        query($login:String!, $from:DateTime!, $to:DateTime!){
            user(login: $login) {
                name
                login
                email
                contributionsCollection(from: $from, to: $to) {
                    totalCommitContributions
                    totalIssueContributions
                    totalPullRequestContributions
                    totalPullRequestReviewContributions
                    totalRepositoryContributions
                    restrictedContributionsCount
                    contributionCalendar {
                        totalContributions
                        weeks {
                            contributionDays {
                                date
                                contributionCount
                                contributionLevel
                            }
                        }
                    }
                }
            }
        }";

        var payload = new
        {
            query,
            variables = new
            {
                login = username,
                from = from.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                to = to.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var httpReq = new HttpRequestMessage(HttpMethod.Post, GraphQlEndpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        // Use request-scoped auth header so different PATs in concurrent scopes don't collide
        httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pat);

        try
        {
            var resp = await _client.SendAsync(httpReq, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub GraphQL call failed: {Status} - {Reason}", resp.StatusCode, resp.ReasonPhrase);
                return (null, [new GitHubGraphQLError { Message = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}" }]);
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var gql = await JsonSerializer.DeserializeAsync<GitHubGraphQLResponse<GitHubUserRoot>>(stream, cancellationToken: ct);
            if (gql?.Errors?.Count > 0)
            {
                _logger.LogWarning("GraphQL returned errors: {Errors}", string.Join(";", gql.Errors.Select(e => e.Message)));
            }
            return (gql?.Data?.User, gql?.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GitHub GraphQL");
            return (null, [new GitHubGraphQLError { Message = ex.Message }]);
        }
    }
}
