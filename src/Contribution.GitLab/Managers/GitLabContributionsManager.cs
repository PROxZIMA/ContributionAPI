using System.Diagnostics;
using Microsoft.Extensions.Options;
using Contribution.Common.Constants;
using Contribution.Common.Models;
using Contribution.Common.Managers;
using Contribution.Common.Helpers;
using Contribution.GitLab.Repository;

namespace Contribution.GitLab.Managers;

public class GitLabContributionsManager(
    IGitLabRepository repository,
    ICacheManager cacheManager,
    IOptions<ContributionsOptions> options) : IGitLabContributionsManager
{
    public async Task<ContributionsResponse> GetContributionsAsync(string username, int year, string pat, bool includeBreakdown, bool includeActivity)
    {
        var stopwatch = Stopwatch.StartNew();

        // Generate cache key based on username, year, and hashed PAT
        var cacheKey = CacheKeyHelper.GenerateGitLabContributionsCacheKey(username, year, pat);
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
        var to = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // initialize days map
        var days = new Dictionary<string, Common.Models.Contribution>();
        for (var d = from; d <= to; d = d.AddDays(1))
            days[d.ToString(SystemConstants.DateFormat)] = new(d.ToString(SystemConstants.DateFormat), 0);

        var (events, error) = await repository.GetUserContributionsAsync(username, from, to, pat);

        if (!string.IsNullOrWhiteSpace(error))
        {
            response.Meta.Errors = [error];
            return response;
        }

        if (events == null || events.Count == 0)
        {
            response.Meta.Errors = [$"No events found for user {username}"];
            return response;
        }

        // Track contribution types for breakdown
        var commitCount = 0;
        var pullRequestCount = 0;
        var issueCount = 0;
        var reviewCount = 0;
        var workItemCount = 0;

        // GitLab contribution logic based on Event model's contributions scope:
        // - Pushed events (action = "pushed")
        // - Commented events (action = "commented on")
        // - CONTRIBUTABLE_TARGET_TYPES with actions: created, closed, merged, approved, updated, destroyed
        //   * MergeRequest
        //   * Issue
        //   * WorkItem
        //   * DesignManagement::Design
        //
        // ref: https://gitlab.com/gitlab-org/gitlab/-/blob/master/app/models/event.rb

        foreach (var evt in events)
        {
            if (string.IsNullOrEmpty(evt.CreatedAt)) continue;

            if (!DateTime.TryParse(evt.CreatedAt, out var eventDate)) continue;

            var dateKey = eventDate.ToString(SystemConstants.DateFormat);
            if (!days.ContainsKey(dateKey)) continue;

            var contribution = days[dateKey];

            // Initialize activity dictionary if needed
            contribution.Activity ??= [];

            var actionName = evt.ActionName?.ToLowerInvariant() ?? "";
            var targetType = evt.TargetType?.ToLowerInvariant() ?? "";

            // 1. COMMITS: Pushed events with commit data
            if (actionName.Contains("pushed") && evt.PushData != null)
            {
                var commits = evt.PushData.CommitCount;
                commitCount += commits;
                contribution.Count += commits;
                contribution.Activity[ContributionTypes.Commits] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.Commits, 0) + commits;
            }
            // 2. MERGE REQUESTS (Pull Requests): MergeRequest target with contributable actions
            // Actions: created, closed, merged, approved, updated, destroyed
            else if (targetType.Contains("mergerequest") &&
                    actionName is "accepted" or "opened" or "closed" or "merged" or "approved" or "updated" or "destroyed")
            {
                pullRequestCount++;
                contribution.Count++;
                contribution.Activity[ContributionTypes.PullRequests] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.PullRequests, 0) + 1;
            }
            // 3. ISSUES: Issue target with contributable actions
            // Actions: created, closed, reopened, updated
            else if (targetType.Contains("issue") &&
                    actionName is "opened" or "closed" or "reopened" or "updated")
            {
                issueCount++;
                contribution.Count++;
                contribution.Activity[ContributionTypes.Issues] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.Issues, 0) + 1;
            }
            // 4. WORK ITEMS: WorkItem target with contributable actions
            // Actions: created, closed, reopened, updated, destroyed
            else if (targetType.Contains("workitem") &&
                    actionName is "opened" or "closed" or "reopened" or "updated" or "destroyed")
            {
                workItemCount++;
                contribution.Count++;
                contribution.Activity[ContributionTypes.WorkItems] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.WorkItems, 0) + 1;
            }
            // 5. REVIEWS: Comments/approvals on MergeRequest
            else if (targetType.Contains("note") &&
                    (evt.Note?.NoteableType?.ToLowerInvariant() ?? "").Contains("mergerequest") &&
                    actionName.Contains("commented"))
            {
                reviewCount++;
                contribution.Count++;
                contribution.Activity[ContributionTypes.Reviews] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.Reviews, 0) + 1;
            }
            // 6. COMMENTED: All other comment/wiki events (on issues, commits, etc.)
            else if (actionName.Contains("commented") ||
                    targetType.Contains("wikipage") ||
                    targetType.Contains("milestone") ||
                    targetType.Contains("project") ||
                    targetType.Contains("design"))
            {
                // Comments are contributions but don't have a specific category
                contribution.Count++;
                contribution.Activity[ContributionTypes.All] =
                    contribution.Activity.GetValueOrDefault(ContributionTypes.All, 0) + 1;
            }
        }

        response.Contributions = [.. days.Values.OrderBy(c => c.Date)];

        // Always store breakdown in cache for filtering later
        response.Breakdown = new Dictionary<string, int>
        {
            {ContributionTypes.Commits, commitCount},
            {ContributionTypes.PullRequests, pullRequestCount},
            {ContributionTypes.Issues, issueCount},
            {ContributionTypes.Reviews, reviewCount},
            {ContributionTypes.WorkItems, workItemCount}
        };

        var totalCount = days.Values.Sum(c => c.Count);
        response.Total[year.ToString()] = totalCount;

        return response;
    }

    private static ContributionsResponse CloneResponseWithFiltering(ContributionsResponse source, bool includeBreakdown, bool includeActivity)
    {
        var result = new ContributionsResponse
        {
            Total = new Dictionary<string, int>(source.Total),
            Meta = new MetaInfo
            {
                ScannedProjects = source.Meta.ScannedProjects,
                ScannedRepos = source.Meta.ScannedRepos,
                Errors = [.. source.Meta.Errors],
            },
            // Clone contributions and filter activity based on request
            Contributions = [.. source.Contributions.Select(c => new Common.Models.Contribution(
                    c.Date,
                    c.Count)
            {
                Activity = includeActivity ? (c.Activity != null ? new Dictionary<string, int>(c.Activity) : null) : null
            })]
        };

        // Include breakdown only if requested
        if (includeBreakdown && source.Breakdown != null)
        {
            result.Breakdown = new Dictionary<string, int>(source.Breakdown);
        }

        return result;
    }
}
