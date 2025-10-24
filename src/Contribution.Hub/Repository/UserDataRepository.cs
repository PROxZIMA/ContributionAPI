using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using Contribution.Hub.Models;
using Contribution.Hub.Services;
using Contribution.Common.Constants;

namespace Contribution.Hub.Repository;

public class UserDataRepository : IUserDataRepository
{
    private readonly FirestoreDb _firestore;
    private readonly string _collectionName;
    private readonly ISecretManagerService _secretManagerService;
    private readonly ILogger<UserDataRepository> _logger;

    public UserDataRepository(
        ISecretManagerService secretManagerService,
        ILogger<UserDataRepository> logger,
        IOptions<HubOptions> options)
    {
        var hubOptions = options.Value;
        _collectionName = hubOptions.Firebase.Collection;
        _secretManagerService = secretManagerService;
        _logger = logger;
        _firestore = new FirestoreDbBuilder
        {
            ProjectId = hubOptions.Firebase.ProjectId,
            DatabaseId = hubOptions.Firebase.Database
        }.Build();
    }

    public async Task<UserData?> GetUserDataAsync(string userId, List<string> supportedProviders)
    {
        try
        {
            var docRef = _firestore.Collection(_collectionName).Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var userData = snapshot.ConvertTo<UserData>();
            userData.Id = snapshot.Id; // Ensure UserId is set from document ID
            var tokens = await GetUserTokensAsync(snapshot.Id);
            foreach (var provider in supportedProviders)
            {
                var lowerProvider = provider.ToLowerInvariant();
                if (tokens.TryGetValue(lowerProvider, out var token))
                {
                    switch (lowerProvider)
                    {
                        case ProviderNames.Azure:
                            if (userData.Azure != null)
                                userData.Azure.Token = token;
                            break;
                        case ProviderNames.GitHub:
                            if (userData.GitHub != null)
                                userData.GitHub.Token = token;
                            break;
                        case ProviderNames.GitLab:
                            if (userData.GitLab != null)
                                userData.GitLab.Token = token;
                            break;
                    }
                }
            }
            return userData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve user data for userId: {userId}", ex);
        }
    }

    private async Task<Dictionary<string, string>> GetUserTokensAsync(string userId)
    {
        try
        {
            var secretKey = $"user-{userId}-tokens";
            var tokenString = await _secretManagerService.GetSecretAsync(secretKey);
            
            var tokens = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(tokenString))
            {
                var tokenPairs = tokenString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in tokenPairs)
                {
                    var parts = pair.Split(':', 2);
                    if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        tokens[parts[0].ToLowerInvariant().Trim()] = parts[1].Trim();
                    }
                }
            }
            
            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tokens for user {UserId}", userId);
            return [];
        }
    }
}