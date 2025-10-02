using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Options;
using Contribution.Hub.Models;

namespace Contribution.Hub.Services;

public class SecretManagerService : ISecretManagerService
{
    private readonly SecretManagerServiceClient _client;
    private readonly string _projectId;

    public SecretManagerService(IOptions<HubOptions> options)
    {
        var hubOptions = options.Value;
        _projectId = hubOptions.Gcp.ProjectId;
        _client = SecretManagerServiceClient.Create();
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secretVersionName = new SecretVersionName(_projectId, secretName, "latest");
            var response = await _client.AccessSecretVersionAsync(secretVersionName);
            return response.Payload.Data.ToStringUtf8();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from GCP Secret Manager", ex);
        }
    }
}