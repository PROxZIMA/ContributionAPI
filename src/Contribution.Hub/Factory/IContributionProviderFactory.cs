using Contribution.Hub.Models;

namespace Contribution.Hub.Factory;

public interface IContributionProviderFactory
{
    Task<ProviderContribution> GetContributionsAsync(
        string providerName,
        UserData userData,
        int year,
        bool includeActivity,
        bool includeBreakdown);

    List<string> SupportedProviders { get; }
}