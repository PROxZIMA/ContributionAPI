using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Contribution.Hub.Models;
using Contribution.Common.Models;

namespace Contribution.Hub.Services;

public class ContributionServiceClient(HttpClient httpClient, IOptions<HubOptions> options) : IContributionServiceClient
{
    private readonly ServiceUrls _serviceUrls = options.Value.ServiceUrls;

    public async Task<ContributionsResponse> GetAzureDevOpsContributionsAsync(
        string email, 
        string organization, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity)
    {
        var url = $"{_serviceUrls.AzureDevOpsApiUrl}/contributions" +
                  $"?email={Uri.EscapeDataString(email)}" +
                  $"&organization={Uri.EscapeDataString(organization)}" +
                  $"&year={year}" +
                  $"&includeBreakdown={includeBreakdown}" +
                  $"&includeActivity={includeActivity}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add Basic Auth header with PAT
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ContributionsResponse>(content) ?? new ContributionsResponse();
    }

    public async Task<ContributionsResponse> GetGitHubContributionsAsync(
        string username, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity)
    {
        var url = $"{_serviceUrls.GitHubApiUrl}/contributions" +
                  $"?username={Uri.EscapeDataString(username)}" +
                  $"&year={year}" +
                  $"&includeBreakdown={includeBreakdown}" +
                  $"&includeActivity={includeActivity}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add Bearer token for GitHub
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", pat);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ContributionsResponse>(content) ?? new ContributionsResponse();
    }

    public async Task<ContributionsResponse> GetGitLabContributionsAsync(
        string username, 
        int year, 
        string pat, 
        bool includeBreakdown, 
        bool includeActivity)
    {
        var url = $"{_serviceUrls.GitLabApiUrl}/contributions" +
                  $"?username={Uri.EscapeDataString(username)}" +
                  $"&year={year}" +
                  $"&includeBreakdown={includeBreakdown}" +
                  $"&includeActivity={includeActivity}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Add Bearer token for GitLab
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", pat);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ContributionsResponse>(content) ?? new ContributionsResponse();
    }
}