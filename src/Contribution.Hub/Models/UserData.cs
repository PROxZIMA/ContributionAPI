using Google.Cloud.Firestore;
using Contribution.Common.Constants ;

namespace Contribution.Hub.Models;

[FirestoreData]
public class UserData
{
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty(ProviderNames.Azure)]
    public AzureDevOpsUserData? Azure { get; set; }

    [FirestoreProperty(ProviderNames.GitHub)]
    public GitHubUserData? GitHub { get; set; }
}

[FirestoreData]
public class AzureDevOpsUserData
{
    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("organization")]
    public string Organization { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

[FirestoreData]
public class GitHubUserData
{
    [FirestoreProperty("username")]
    public string Username { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}