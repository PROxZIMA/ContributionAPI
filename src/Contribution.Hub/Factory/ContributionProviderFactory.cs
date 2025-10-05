using Contribution.Hub.Models;
using Contribution.Hub.Services;
using Contribution.Common.Constants;

namespace Contribution.Hub.Factory;

public class ContributionProviderFactory : IContributionProviderFactory
{
    private readonly IContributionServiceClient _serviceClient;
    private readonly ILogger<ContributionProviderFactory> _logger;

    // Delegate for provider implementation functions
    private delegate Task<ProviderContribution> ProviderImplementation(UserData userData, int year, bool includeActivity, bool includeBreakdown);
    
    // Dictionary mapping provider names to their implementations
    private readonly Dictionary<string, ProviderImplementation> _providerImplementations;

    public List<string> SupportedProviders => [.. _providerImplementations.Keys];

    public ContributionProviderFactory(
        IContributionServiceClient serviceClient,
        ILogger<ContributionProviderFactory> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;

        // Initialize provider implementations dictionary
        // Not a actual factory pattern, but a simple mapping
        _providerImplementations = new Dictionary<string, ProviderImplementation>
        {
            { ProviderNames.Azure, GetAzureDevOpsContributionsAsync },
            { ProviderNames.GitHub, GetGitHubContributionsAsync }
        };
    }

    public async Task<ProviderContribution> GetContributionsAsync(
        string providerName,
        UserData userData,
        int year,
        bool includeActivity,
        bool includeBreakdown)
    {
        var normalizedProviderName = providerName.ToLowerInvariant();
        
        // Check if provider implementation exists
        if (!_providerImplementations.TryGetValue(normalizedProviderName, out var implementation))
        {
            return CreateErrorResult(providerName, "Unknown provider");
        }
        
        return await implementation(userData, year, includeActivity, includeBreakdown);
    }

    private static ProviderContribution CreateProviderNotFoundError(string providerName)
    {
        return CreateErrorResult(providerName, $"Provider '{providerName}' not found in user data");
    }

    private async Task<ProviderContribution> GetAzureDevOpsContributionsAsync(
        UserData userData,
        int year,
        bool includeActivity,
        bool includeBreakdown)
    {
        // Check if Azure data exists
        if (userData.Azure == null)
        {
            return CreateProviderNotFoundError(ProviderNames.Azure);
        }

        var providerContribution = new ProviderContribution
        {
            Provider = ProviderNames.Azure
        };

        try
        {
            var contributionsResponse = await _serviceClient.GetAzureDevOpsContributionsAsync(
                userData.Azure.Email,
                userData.Azure.Organization,
                year,
                userData.Azure.Token,
                includeBreakdown,
                includeActivity);

            providerContribution.IsSuccessful = true;
            providerContribution.Data = contributionsResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Azure DevOps contributions for email {Email}", userData.Azure.Email);
            providerContribution.IsSuccessful = false;
            providerContribution.ErrorMessage = ex.Message;
        }

        return providerContribution;
    }

    private async Task<ProviderContribution> GetGitHubContributionsAsync(
        UserData userData,
        int year,
        bool includeActivity,
        bool includeBreakdown)
    {
        // Check if GitHub data exists
        if (userData.GitHub == null)
        {
            return CreateProviderNotFoundError(ProviderNames.GitHub);
        }

        var providerContribution = new ProviderContribution
        {
            Provider = ProviderNames.GitHub
        };

        try
        {
            var contributionsResponse = await _serviceClient.GetGitHubContributionsAsync(
                userData.GitHub.Username,
                year,
                userData.GitHub.Token,
                includeBreakdown,
                includeActivity);

            providerContribution.IsSuccessful = true;
            providerContribution.Data = contributionsResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub contributions for username {Username}", userData.GitHub.Username);
            providerContribution.IsSuccessful = false;
            providerContribution.ErrorMessage = ex.Message;
        }

        return providerContribution;
    }

    private static ProviderContribution CreateErrorResult(string providerName, string errorMessage)
    {
        return new ProviderContribution
        {
            Provider = providerName,
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}