using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureContributionsApi.Factory;

public interface IAzureClientFactory
{
    VssConnection GetConnection(string organization, string pat);
    Task<GitHttpClient> GetGitClient(string organization, string pat);
    Task<ProjectHttpClient> GetProjectClient(string organization, string pat);
    Task<WorkItemTrackingHttpClient> GetWorkItemClient(string organization, string pat);
}