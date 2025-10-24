using System.Security.Cryptography;
using System.Text;

namespace Contribution.Common.Helpers;

public static class CacheKeyHelper
{
    public static string GenerateAzureContributionsCacheKey(string email, string organization, int year, string pat)
    {
        // Hash the PAT for security - we don't want to store PATs in cache keys
        var patHash = ComputeHash(pat);
        return $"azure-contributions:{email}:{organization}:{year}:{patHash}";
    }

    public static string GenerateGitHubContributionsCacheKey(string username, int year, string pat)
    {
        // Hash the PAT for security - we don't want to store PATs in cache keys
        var patHash = ComputeHash(pat);
        return $"github-contributions:{username}:{year}:{patHash}";
    }

    public static string GenerateGitLabContributionsCacheKey(string username, int year, string pat)
    {
        // Hash the PAT for security - we don't want to store PATs in cache keys
        var patHash = ComputeHash(pat);
        return $"gitlab-contributions:{username}:{year}:{patHash}";
    }

    private static string ComputeHash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes)[..16]; // Take first 16 characters for brevity
    }
}