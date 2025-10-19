using System.Diagnostics;
using Microsoft.Extensions.Options;
using Contribution.Common.Constants;
using Contribution.Common.Models;
using Contribution.Common.Managers;
using Contribution.Common.Helpers;
using Contribution.GitHub.Repository;

namespace Contribution.GitHub.Managers;

public class GitHubContributionsManager(
    IGitHubRepository repository, 
    ILogger<GitHubContributionsManager> logger,
    ICacheManager cacheManager,
    IOptions<ContributionsOptions> options) : IGitHubContributionsManager
{
    public async Task<ContributionsResponse> GetContributionsAsync(string username, int year, string pat, bool includeBreakdown, bool includeActivity)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Generate cache key based on username, year, and hashed PAT
        var cacheKey = CacheKeyHelper.GenerateGitHubContributionsCacheKey(username, year, pat);
        var cacheExpiration = TimeSpan.FromMinutes(options.Value.ContributionsCacheMinutes);
        
        // Try to get cached response with full breakdown and activity
        var cacheResult = await cacheManager.GetOrSetWithStatusAsync(cacheKey, async () =>
        {
            return await GenerateContributionsResponseAsync(username, year, pat);
        }, cacheExpiration);

        if (cacheResult.Value == null)
        {
            var errorResponse = new ContributionsResponse();
            errorResponse.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
            errorResponse.Meta.CacheHit = cacheResult.IsHit;
            errorResponse.Meta.Errors = [$"Could not resolve user or contributions for {username}"];
            return errorResponse;
        }

        // Check if the cached response has errors but no contributions (failed request)
        if (cacheResult.Value.Contributions.Count == 0 && cacheResult.Value.Meta.Errors.Count > 0)
        {
            // Return the error response as-is, just update metadata
            var errorResponse = CloneResponseWithFiltering(cacheResult.Value, includeBreakdown, includeActivity);
            errorResponse.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
            errorResponse.Meta.CacheHit = cacheResult.IsHit;
            return errorResponse;
        }

        // Clone the cached response and filter based on requested options
        var response = CloneResponseWithFiltering(cacheResult.Value, includeBreakdown, includeActivity);
        response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
        response.Meta.CacheHit = cacheResult.IsHit;
        
        return response;
    }

    private async Task<ContributionsResponse?> GenerateContributionsResponseAsync(string username, int year, string pat)
    {
        var response = new ContributionsResponse();

        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // GitHub requires range <= 1 year. Use inclusive last second of the year.
        var to = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // initialize days map
        var days = new Dictionary<string, Common.Models.Contribution>();
        for (var d = from; d < to; d = d.AddDays(1))
            days[d.ToString(SystemConstants.DateFormat)] = new(d.ToString(SystemConstants.DateFormat), 0);

        var errors = new List<string>();

        var (user, gqlErrors) = await repository.GetUserContributionsAsync(username, from, to, pat);

        if (gqlErrors != null && gqlErrors.Count > 0)
        {
            foreach (var ge in gqlErrors)
            {
                if (!string.IsNullOrWhiteSpace(ge.Message))
                {
                    var path = ge.Path != null ? string.Join('.', ge.Path) : null;
                    var locations = ge.Locations != null ? string.Join(',', ge.Locations.Select(l => $"(Line {l.Line}, Column {l.Column})")) : null;
                    var errorType = !string.IsNullOrWhiteSpace(ge.Type) ? $"[{ge.Type}] " : "";
                    var errorMessage = path == null
                        ? (locations == null ? ge.Message! : $"{ge.Message} at {locations}")
                        : (locations == null ? $"({path}): {ge.Message}" : $"({path}): {ge.Message} at {locations}");
                    errors.Add($"{errorType}{errorMessage}".Trim());
                }
            }
        }

        if (user?.ContributionsCollection == null)
        {
            if (errors.Count == 0)
                errors.Add($"Could not resolve user or contributions for {username}");
            
            // Return response with errors instead of null to properly report GraphQL errors
            response.Meta.Errors = errors;
            return response;
        }

        var collection = user.ContributionsCollection;
        var calendar = collection.ContributionCalendar;
        
        if (calendar != null)
        {
            foreach (var week in calendar.Weeks)
            {
                foreach (var day in week.ContributionDays)
                {
                    var key = day.Date.ToString(SystemConstants.DateFormat);
                    if (!days.TryGetValue(key, out var existing))
                    {
                        existing = new Common.Models.Contribution(key, 0);
                        days[key] = existing;
                    }
                    existing.Count += day.ContributionCount;
                    
                    // Always populate activity in cached response for filtering later
                    existing.Activity ??= [];
                    // GitHub calendar lumps all contributions; we treat as "all" activity type
                    existing.Activity[ContributionTypes.All] = existing.Count;
                }
            }
        }

        // Always store breakdown in cache for filtering later
        response.Breakdown = new Dictionary<string, int>
        {
            {ContributionTypes.Commits, collection.TotalCommitContributions},
            {ContributionTypes.Issues, collection.TotalIssueContributions},
            {ContributionTypes.PullRequests, collection.TotalPullRequestContributions},
            {ContributionTypes.Reviews, collection.TotalPullRequestReviewContributions},
            {ContributionTypes.Restricted, collection.RestrictedContributionsCount}
        };

        var totalCount = days.Values.Sum(c => c.Count);
        response.Total[year.ToString()] = totalCount;
        response.Contributions = [.. days.Values.OrderBy(d => d.Date)];

        response.Meta.Errors = errors;
        response.Meta.ScannedRepos = collection.TotalRepositoryContributions;

        return response;
    }

    private static ContributionsResponse CloneResponseWithFiltering(ContributionsResponse cached, bool includeBreakdown, bool includeActivity)
    {
        var response = new ContributionsResponse
        {
            Total = new Dictionary<string, int>(cached.Total),
            Meta = new MetaInfo
            {
                ScannedProjects = cached.Meta.ScannedProjects,
                ScannedRepos = cached.Meta.ScannedRepos,
                Errors = [.. cached.Meta.Errors],
            },
            // Clone contributions and filter activity based on request
            Contributions = [.. cached.Contributions.Select(c => new Common.Models.Contribution(
                    c.Date,
                    c.Count)
            {
                Activity = includeActivity ? (c.Activity != null ? new Dictionary<string, int>(c.Activity) : null) : null
            })]
        };

        // Include breakdown only if requested
        if (includeBreakdown && cached.Breakdown != null)
        {
            response.Breakdown = new Dictionary<string, int>(cached.Breakdown);
        }

        return response;
    }
}
