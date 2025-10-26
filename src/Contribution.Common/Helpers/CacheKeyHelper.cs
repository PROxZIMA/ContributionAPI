using System.Security.Cryptography;
using System.Text;

namespace Contribution.Common.Helpers;

public static class CacheKeyHelper
{
    public static string GenerateAzureContributionsCacheKey(string email, string organization, int year, string pat)
    {
        var patHash = ComputeHash(pat);
        var emailHash = ComputeHash(email);
        var orgHash = ComputeHash(organization);
        return $"azure-contributions:{emailHash}:{orgHash}:{year}:{patHash}";
    }

    public static string GenerateGitHubContributionsCacheKey(string username, int year, string pat)
    {
        var patHash = ComputeHash(pat);
        var usernameHash = ComputeHash(username);
        return $"github-contributions:{usernameHash}:{year}:{patHash}";
    }

    public static string GenerateGitLabContributionsCacheKey(string username, int year, string pat)
    {
        var patHash = ComputeHash(pat);
        var usernameHash = ComputeHash(username);
        return $"gitlab-contributions:{usernameHash}:{year}:{patHash}";
    }

    private static string ComputeHash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes)[..16]; // Take first 16 characters for brevity
    }
}