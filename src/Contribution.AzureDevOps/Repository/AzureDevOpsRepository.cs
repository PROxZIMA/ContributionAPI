using Contribution.Common.Models;
using Contribution.AzureDevOps.Factory;
using Contribution.AzureDevOps.Managers;
using Contribution.AzureDevOps.Models;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity;
using Newtonsoft.Json;

namespace Contribution.AzureDevOps.Repository;

public sealed class AzureDevOpsRepository(
    IAzureDevOpsCacheManager cacheManager,
    IHttpClientFactory httpClientFactory,
    IAzureClientFactory azureClientFactory,
    IOptions<ContributionsOptions> options,
    ILogger<AzureDevOpsRepository> logger) : IAzureDevOpsRepository
{
    private readonly IAzureDevOpsCacheManager _cacheManager = cacheManager;
    private readonly HttpClient _client = httpClientFactory.CreateClient("azuredevops-rest");
    private readonly IAzureClientFactory _azureClientFactory = azureClientFactory;
    private readonly IOptions<ContributionsOptions> _options = options;
    private readonly ILogger<AzureDevOpsRepository> _logger = logger;

    public async Task<Identity?> GetIdentityAsync(string organization, string email, string pat)
    {
        var cacheKey = $"identity:{organization}:{email}";
        return await _cacheManager.GetOrSetAsync(cacheKey, async () =>
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"https://vssps.dev.azure.com/{organization}/_apis/identities?searchFilter=MailAddress&filterValue={Uri.EscapeDataString(email)}&api-version=7.1");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));

            var response = await _client.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Identity lookup failed for {email} with {code}", email, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<AzureApiResponse<Identity[]>>(content);

            if (apiResponse?.Value != null && apiResponse.Value.Length != 0)
            {
                var identity = apiResponse.Value.First();
                if (!string.IsNullOrEmpty(identity.SubjectDescriptor))
                {
                    return identity;
                }
            }

            return null;
        }, TimeSpan.FromMinutes(_options.Value.IdentityCacheMinutes));
    }

    public async Task<IReadOnlyCollection<TeamProjectReference>> GetProjectsAsync(string organization, string pat)
    {
        var cacheKey = $"projects:{organization}";
        return await _cacheManager.GetOrSetCollectionAsync<TeamProjectReference>(cacheKey, async () =>
        {
            var projectClient = await _azureClientFactory.GetProjectClient(organization, pat);
            return await projectClient.GetProjects();
        }, TimeSpan.FromMinutes(_options.Value.ProjectsCacheMinutes));
    }

    public async Task<IReadOnlyCollection<GitRepository>> GetRepositoriesAsync(string organization, string pat, TeamProjectReference project)
    {
        var cacheKey = $"repos:{project.Id}";
        return await _cacheManager.GetOrSetCollectionAsync<GitRepository>(cacheKey, async () =>
        {
            var gitClient = await _azureClientFactory.GetGitClient(organization, pat);
            return await gitClient.GetRepositoriesAsync(project.Id);
        }, TimeSpan.FromMinutes(_options.Value.RepoCacheMinutes));
    }
}