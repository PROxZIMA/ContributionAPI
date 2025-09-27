using Microsoft.VisualStudio.Services.Identity;
using Contribution.Common.Models;

namespace Contribution.AzureDevOps.Strategy;

public interface IContributionStrategy
{
    Task<IReadOnlyDictionary<string, Common.Models.Contribution>> GetContributionsAsync(
        Identity userIdentity,
        string organization,
        string pat,
        DateTime fromDate,
        DateTime toDate);
}