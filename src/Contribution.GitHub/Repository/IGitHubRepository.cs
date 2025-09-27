using Contribution.GitHub.Models;

namespace Contribution.GitHub.Repository;

public interface IGitHubRepository
{
    Task<(GitHubUser? User, List<GitHubGraphQLError>? Errors)> GetUserContributionsAsync(string username, DateTime from, DateTime to, string pat, CancellationToken ct = default);
}
