using AzureContributionsApi.Models;
using Microsoft.VisualStudio.Services.Identity;

namespace AzureContributionsApi.Strategy;

public interface IContributionStrategy
{
    Task<IReadOnlyDictionary<string, Contribution>> GetContributionsAsync(
        Identity userIdentity,
        string organization,
        string pat,
        DateTime fromDate,
        DateTime toDate);
}