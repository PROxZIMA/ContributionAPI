using Contribution.Common.Models;
using Contribution.Hub.Models;

namespace Contribution.Hub.Managers;

public interface IContributionAggregatorManager
{
    Task<ContributionsResponse> GetAggregatedContributionsAsync(
        string userId, 
        int year,
        string[]? providers = null,
        bool includeActivity = false, 
        bool includeBreakdown = false);

    Task<ContributionsResponse> GetAggregatedContributionsAsync(
        UserData userData, 
        int year,
        string[]? providers = null,
        bool includeActivity = false, 
        bool includeBreakdown = false);
}