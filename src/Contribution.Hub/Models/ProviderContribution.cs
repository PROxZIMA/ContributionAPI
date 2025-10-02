using Contribution.Common.Models;

namespace Contribution.Hub.Models;

public class ProviderContribution
{
    public string Provider { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public ContributionsResponse? Data { get; set; }
}