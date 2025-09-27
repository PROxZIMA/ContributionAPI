using Contribution.Common.Models;

namespace Contribution.AzureDevOps.Managers;

public interface IContributionsManager
{
    Task<ContributionsResponse> GetContributionsAsync(string email, string organization, int year, string pat, bool includeBreakdown, bool includeActivity);
}
