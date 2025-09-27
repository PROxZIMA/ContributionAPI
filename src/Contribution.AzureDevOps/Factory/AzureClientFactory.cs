using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Concurrent;

namespace Contribution.AzureDevOps.Factory;

public sealed class AzureClientFactory : IAzureClientFactory
{
    private readonly ConcurrentDictionary<string, VssConnection> _connections = new();

    public VssConnection GetConnection(string organization, string pat)
    {
        ArgumentException.ThrowIfNullOrEmpty(organization);
        ArgumentException.ThrowIfNullOrEmpty(pat);

        var connectionKey = $"{organization}:{pat.GetHashCode()}";
        return _connections.GetOrAdd(connectionKey, _ =>
        {
            var credentials = new VssBasicCredential(string.Empty, pat);
            return new VssConnection(new Uri($"https://dev.azure.com/{organization}"), credentials);
        });
    }

    public async Task<GitHttpClient> GetGitClient(string organization, string pat)
    {
        var connection = GetConnection(organization, pat);
        return await connection.GetClientAsync<GitHttpClient>();
    }

    public async Task<ProjectHttpClient> GetProjectClient(string organization, string pat)
    {
        var connection = GetConnection(organization, pat);
        return await connection.GetClientAsync<ProjectHttpClient>();
    }

    public async Task<WorkItemTrackingHttpClient> GetWorkItemClient(string organization, string pat)
    {
        var connection = GetConnection(organization, pat);
        return await connection.GetClientAsync<WorkItemTrackingHttpClient>();
    }
}