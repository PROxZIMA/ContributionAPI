using Contribution.Common.Models;

namespace Contribution.GitLab.Managers;

public interface IGitLabContributionsManager
{
    Task<ContributionsResponse> GetContributionsAsync(
        string username, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity);
}
