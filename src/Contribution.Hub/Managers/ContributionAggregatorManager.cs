using Contribution.Hub.Models;
using Contribution.Hub.Repository;
using Contribution.Hub.Factory;
using Contribution.Common.Models;

namespace Contribution.Hub.Managers;

public class ContributionAggregatorManager(
    IUserDataRepository userDataRepository,
    IContributionProviderFactory providerFactory,
    ILogger<ContributionAggregatorManager> logger) : IContributionAggregatorManager
{
    private readonly IUserDataRepository _userDataRepository = userDataRepository;
    private readonly IContributionProviderFactory _providerFactory = providerFactory;
    private readonly ILogger<ContributionAggregatorManager> _logger = logger;

    public async Task<ContributionsResponse> GetAggregatedContributionsAsync(
        string userId, 
        int year,
        string[]? providers = null,
        bool includeActivity = false, 
        bool includeBreakdown = false)
    {
        // Get user data from Firestore
        var userData = await _userDataRepository.GetUserDataAsync(userId, _providerFactory.SupportedProviders) ?? throw new ArgumentException($"User data not found for userId: {userId}");
        return await GetAggregatedContributionsAsync(userData, year, providers, includeActivity, includeBreakdown);
    }
    
    public async Task<ContributionsResponse> GetAggregatedContributionsAsync(
        UserData userData,
        int year,
        string[]? providers = null,
        bool includeActivity = false,
        bool includeBreakdown = false)
    {
        var mergedResponse = new ContributionsResponse
        {
            Total = [],
            Contributions = [],
            Breakdown = includeBreakdown ? [] : null,
            Meta = new MetaInfo
            {
                Errors = []
            }
        };

        try
        {
            // Use provided providers or default to all available providers
            var requestedProviders = providers?.Where(p => !string.IsNullOrWhiteSpace(p))
                                              .Select(p => p.ToLowerInvariant())
                                              .ToList()
                                    ?? _providerFactory.SupportedProviders;

            if (requestedProviders.Count == 0)
            {
                _logger.LogWarning("No providers specified for user {UserId}", userData.Id);
                mergedResponse.Meta.Errors.Add("No providers specified");
                return mergedResponse;
            }

            // Fetch contributions from requested providers in parallel using factory
            var tasks = requestedProviders.Select(provider =>
                _providerFactory.GetContributionsAsync(provider, userData, year, includeActivity, includeBreakdown)
            );

            // Wait for all tasks to complete
            var providerResults = await Task.WhenAll(tasks);

            // Merge all responses (including errors for missing providers)
            MergeProviderResponses(mergedResponse, providerResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to aggregate contributions for user {UserId}", userData.Id);
            mergedResponse.Meta.Errors.Add($"Aggregation failed: {ex.Message}");
        }

        return mergedResponse;
    }

    private static void MergeProviderResponses(ContributionsResponse mergedResponse, ProviderContribution[] providerResults)
    {
        var contributionsByDate = new Dictionary<string, Common.Models.Contribution>();
        
        foreach (var provider in providerResults)
        {
            if (!provider.IsSuccessful)
            {
                mergedResponse.Meta.Errors.Add($"{provider.Provider}: {provider.ErrorMessage}");
                continue;
            }

            if (provider.Data == null) continue;

            var data = provider.Data;

            // Merge Total dictionary key-wise
            if (data.Total != null)
            {
                foreach (var kvp in data.Total)
                {
                    if (mergedResponse.Total.ContainsKey(kvp.Key))
                    {
                        mergedResponse.Total[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        mergedResponse.Total[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Merge Breakdown dictionary key-wise
            if (data.Breakdown != null && mergedResponse.Breakdown != null)
            {
                foreach (var kvp in data.Breakdown)
                {
                    if (mergedResponse.Breakdown.ContainsKey(kvp.Key))
                    {
                        mergedResponse.Breakdown[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        mergedResponse.Breakdown[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Merge Contributions by date
            if (data.Contributions != null)
            {
                foreach (var contribution in data.Contributions)
                {
                    if (contributionsByDate.TryGetValue(contribution.Date, out Common.Models.Contribution? value))
                    {
                        value.Count += contribution.Count;
                        
                        // Merge activity if present
                        if (contribution.Activity != null && contributionsByDate[contribution.Date].Activity != null)
                        {
                            var existingActivity = contributionsByDate[contribution.Date].Activity!;
                            foreach (var activityKvp in contribution.Activity)
                            {
                                if (existingActivity.ContainsKey(activityKvp.Key))
                                {
                                    existingActivity[activityKvp.Key] += activityKvp.Value;
                                }
                                else
                                {
                                    existingActivity[activityKvp.Key] = activityKvp.Value;
                                }
                            }
                        }
                        else if (contribution.Activity != null)
                        {
                            contributionsByDate[contribution.Date].Activity = new Dictionary<string, int>(contribution.Activity);
                        }
                    }
                    else
                    {
                        // Create new contribution for this date
                        contributionsByDate[contribution.Date] = new Common.Models.Contribution(contribution.Date, contribution.Count)
                        {
                            Activity = contribution.Activity != null ? new Dictionary<string, int>(contribution.Activity) : null
                        };
                    }
                }
            }

            // Merge Meta information
            mergedResponse.Meta.ScannedProjects += data.Meta?.ScannedProjects ?? 0;
            mergedResponse.Meta.ScannedRepos += data.Meta?.ScannedRepos ?? 0;
            mergedResponse.Meta.ElapsedMs += data.Meta?.ElapsedMs ?? 0;
            
            if (data.Meta?.CacheHit == true)
            {
                mergedResponse.Meta.CacheHit = true;
            }

            if (data.Meta?.Errors != null)
            {
                foreach (var error in data.Meta.Errors)
                {
                    mergedResponse.Meta.Errors.Add($"{provider.Provider}: {error}");
                }
            }
        }

        // Convert contributions dictionary back to list, sorted by date
        mergedResponse.Contributions = [.. contributionsByDate.Values.OrderBy(c => c.Date)];
    }
}