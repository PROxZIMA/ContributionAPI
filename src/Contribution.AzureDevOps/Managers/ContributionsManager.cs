using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Contribution.Common.Constants;
using Contribution.Common.Models;
using Contribution.Common.Attributes;
using Contribution.AzureDevOps.Repository;
using Contribution.AzureDevOps.Strategy;

namespace Contribution.AzureDevOps.Managers;

public class ContributionsManager(
    IAzureDevOpsRepository repository,
    ILogger<ContributionsManager> logger,
    IEnumerable<IContributionStrategy> contributionStrategies) : IContributionsManager
{
    public async Task<ContributionsResponse> GetContributionsAsync(string email, string organization, int year, string pat, bool includeBreakdown, bool includeActivity)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new ContributionsResponse();

        // validate year -> build UTC range
        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Prepare per-day dictionary initialized to 0 for the entire year
        var days = new Dictionary<string, Common.Models.Contribution>();
        for (var d = from; d <= to; d = d.AddDays(1))
            days[d.ToString(SystemConstants.DateFormat)] = new(d.ToString(SystemConstants.DateFormat), 0);

        // 1) Resolve identity using repository
        var identity = await repository.GetIdentityAsync(organization, email, pat);
        if (identity == null)
        {
            response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
            response.Meta.Errors = [ $"Could not resolve identity for {email} in {organization}" ];
            return response;
        }

        // 2) Get projects count for metadata
        var projects = await repository.GetProjectsAsync(organization, pat);
        response.Meta.ScannedProjects = projects.Count;

        var errors = new ConcurrentBag<string>();
        var breakdownCounts = new ConcurrentDictionary<string, int>();

        // 3) Execute all contribution strategies in parallel
        var strategyTasks = contributionStrategies.Select(async strategy =>
        {
            try
            {
                // Get contribution type from attribute
                var contributionTypeAttr = strategy.GetType().GetCustomAttribute<ContributionTypeAttribute>();
                var contributionType = contributionTypeAttr?.ContributionType ?? strategy.GetType().Name.Replace("ContributionStrategy", "").ToLowerInvariant();
                
                var contributions = await strategy.GetContributionsAsync(identity, organization, pat, from, to);
                
                // Merge strategy results into main dictionary
                foreach (var kvp in contributions)
                {
                    lock (days)
                    {
                        if (days.TryGetValue(kvp.Key, out var existing))
                        {
                            existing.Count += kvp.Value.Count;
                        }
                        // Should not happen, but just in case
                        else
                        {
                            days[kvp.Key] = new Common.Models.Contribution(kvp.Key, kvp.Value.Count);
                        }

                        if (includeActivity)
                        {
                            days[kvp.Key].Activity ??= [];
                            days[kvp.Key].Activity![contributionType] = kvp.Value.Count;
                        }
                    }
                }

                // Track breakdown by strategy type using constant
                var strategyTotal = contributions.Values.Sum(c => c.Count);
                breakdownCounts.TryAdd(contributionType, strategyTotal);
            }
            catch (Exception ex)
            {
                var contributionTypeAttr = strategy.GetType().GetCustomAttribute<ContributionTypeAttribute>();
                var contributionType = contributionTypeAttr?.ContributionType ?? strategy.GetType().Name;
                logger.LogWarning(ex, "Error executing strategy {Strategy}", contributionType);
                errors.Add($"strategy:{contributionType}, reason:{ex.Message}");
            }
        });

        await Task.WhenAll(strategyTasks);

        // Calculate repository count from first strategy (they all scan the same repos)
        var firstStrategy = contributionStrategies.FirstOrDefault();
        if (firstStrategy != null)
        {
            var allProjects = await repository.GetProjectsAsync(organization, pat);
            var totalRepoCount = 0;
            foreach (var project in allProjects)
            {
                var repos = await repository.GetRepositoriesAsync(organization, pat, project);
                totalRepoCount += repos.Count;
            }
            response.Meta.ScannedRepos = totalRepoCount;
        }

        // Aggregate totals & construct response
        var totalCount = days.Values.Sum(c => c.Count);
        response.Total[year.ToString()] = totalCount;

        response.Contributions = [.. days.Values.OrderBy(d => d.Date)];

        if (includeBreakdown)
        {
            response.Breakdown = breakdownCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
        response.Meta.Errors = [.. errors];
        response.Meta.CachedProjects = false;

        return response;
    }
}
