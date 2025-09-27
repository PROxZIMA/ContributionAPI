using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity;

namespace AzureContributionsApi.Repository;

public interface IAzureDevOpsRepository
{
    Task<Identity?> GetIdentityAsync(string organization, string email, string pat);
    Task<IReadOnlyCollection<TeamProjectReference>> GetProjectsAsync(string organization, string pat);
    Task<IReadOnlyCollection<GitRepository>> GetRepositoriesAsync(string organization, string pat, TeamProjectReference project);
}