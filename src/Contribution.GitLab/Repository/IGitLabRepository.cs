using Contribution.GitLab.Models;

namespace Contribution.GitLab.Repository;

public interface IGitLabRepository
{
    Task<(List<GitLabEvent>? Events, string? Error)> GetUserContributionsAsync(
        string username,
        DateTime from,
        DateTime to,
        string pat,
        CancellationToken ct = default);
}