using AzureContributionsApi.Models;

namespace AzureContributionsApi.Managers;

public interface IContributionsManager
{
    Task<ContributionsResponse> GetContributionsAsync(string email, string organization, int year, string pat, bool includeBreakdown);
}
