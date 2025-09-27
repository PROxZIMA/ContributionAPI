using System.Diagnostics;
using Contribution.Common.Constants;
using Contribution.Common.Models;
using Contribution.GitHub.Repository;

namespace Contribution.GitHub.Managers;

public class GitHubContributionsManager(IGitHubRepository repository, ILogger<GitHubContributionsManager> logger) : IGitHubContributionsManager
{
    public async Task<ContributionsResponse> GetContributionsAsync(string username, int year, string pat, bool includeBreakdown, bool includeActivity)
    {
        var stopwatch = Stopwatch.StartNew();
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
                    errors.Add(path == null ? ge.Message! : $"{path}: {ge.Message}");
                }
            }
        }

        if (user?.ContributionsCollection == null)
        {
            if (errors.Count == 0)
                errors.Add($"Could not resolve user or contributions for {username}");
            response.Meta.Errors = errors;
            response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
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
                    if (includeActivity)
                    {
                        existing.Activity ??= [];
                        // GitHub calendar lumps all contributions; we treat as "all" activity type
                        existing.Activity["all"] = existing.Count;
                    }
                }
            }
        }

        // Build breakdown if requested from aggregate counters
        if (includeBreakdown)
        {
            response.Breakdown = new Dictionary<string, int>
            {
                {"commits", collection.TotalCommitContributions},
                {"issues", collection.TotalIssueContributions},
                {"pullrequests", collection.TotalPullRequestContributions},
                {"reviews", collection.TotalPullRequestReviewContributions},
                {"restricted", collection.RestrictedContributionsCount}
            };
        }

        var totalCount = days.Values.Sum(c => c.Count);
        response.Total[year.ToString()] = totalCount;
        response.Contributions = [.. days.Values.OrderBy(d => d.Date)];

        response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
        response.Meta.Errors = errors;
        response.Meta.ScannedRepos = collection.TotalRepositoryContributions;

        return response;
    }
}
