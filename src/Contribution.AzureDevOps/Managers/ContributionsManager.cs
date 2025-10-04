using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;
using Contribution.Common.Constants;
using Contribution.Common.Models;
using Contribution.Common.Attributes;
using Contribution.Common.Managers;
using Contribution.Common.Helpers;
using Contribution.AzureDevOps.Repository;
using Contribution.AzureDevOps.Strategy;

namespace Contribution.AzureDevOps.Managers;

public class ContributionsManager(
    IAzureDevOpsRepository repository,
    ILogger<ContributionsManager> logger,
    IEnumerable<IContributionStrategy> contributionStrategies,
    ICacheManager cacheManager,
    IOptions<ContributionsOptions> options) : IContributionsManager
{
    public async Task<ContributionsResponse> GetContributionsAsync(string email, string organization, int year, string pat, bool includeBreakdown, bool includeActivity)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Generate cache key based on email, org, year, and hashed PAT
        var cacheKey = CacheKeyHelper.GenerateAzureContributionsCacheKey(email, organization, year, pat);
        var cacheExpiration = TimeSpan.FromMinutes(options.Value.ContributionsCacheMinutes);
        
        // Try to get cached response with full breakdown and activity
        var cacheResult = await cacheManager.GetOrSetWithStatusAsync(cacheKey, async () =>
        {
            return await GenerateContributionsResponseAsync(email, organization, year, pat);
        }, cacheExpiration);

        if (cacheResult.Value == null)
        {
            var errorResponse = new ContributionsResponse();
            errorResponse.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
            errorResponse.Meta.CacheHit = cacheResult.IsHit;
            errorResponse.Meta.Errors = [$"Could not resolve identity for {email} in {organization}"];
            return errorResponse;
        }

        // Clone the cached response and filter based on requested options
        var response = CloneResponseWithFiltering(cacheResult.Value, includeBreakdown, includeActivity);
        response.Meta.ElapsedMs = stopwatch.ElapsedMilliseconds;
        response.Meta.CacheHit = cacheResult.IsHit;
        
        return response;
    }

    private async Task<ContributionsResponse?> GenerateContributionsResponseAsync(string email, string organization, int year, string pat)
    {
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
            return null;
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

                        // Always populate activity in cached response for filtering later
                        days[kvp.Key].Activity ??= [];
                        days[kvp.Key].Activity![contributionType] = kvp.Value.Count;
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

        // Always store breakdown in cache for filtering later
        response.Breakdown = breakdownCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        response.Meta.Errors = [.. errors];

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
