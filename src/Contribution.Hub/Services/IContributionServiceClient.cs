using Contribution.Common.Models;

namespace Contribution.Hub.Services;

public interface IContributionServiceClient
{
    Task<ContributionsResponse> GetAzureDevOpsContributionsAsync(
        string email, 
        string organization, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity);

    Task<ContributionsResponse> GetGitHubContributionsAsync(
        string username, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity);

    Task<ContributionsResponse> GetGitLabContributionsAsync(
        string username, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity);
}