using Contribution.Common.Models;

namespace Contribution.GitHub.Managers;

public interface IGitHubContributionsManager
{
    Task<ContributionsResponse> GetContributionsAsync(string username, int year, string pat, bool includeBreakdown, bool includeActivity);
}
