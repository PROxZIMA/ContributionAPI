namespace Contribution.Hub.Services;

public interface ISecretManagerService
{
    Task<string> GetSecretAsync(string secretName);
}