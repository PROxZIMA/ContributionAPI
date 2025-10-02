namespace Contribution.Hub.Models;

public class HubOptions
{
    public const string SectionName = "Hub";
    
    public ServiceUrls ServiceUrls { get; set; } = new();
    public GcpOptions Gcp { get; set; } = new();
    public FirebaseOptions Firebase { get; set; } = new();
}

public class ServiceUrls
{
    public string AzureDevOpsApiUrl { get; set; } = string.Empty;
    public string GitHubApiUrl { get; set; } = string.Empty;
}

public class GcpOptions
{
    public string ProjectId { get; set; } = string.Empty;
}

public class FirebaseOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
}