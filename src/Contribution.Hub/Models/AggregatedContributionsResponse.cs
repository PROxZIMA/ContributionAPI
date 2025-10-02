using Contribution.Common.Models;

namespace Contribution.Hub.Models;

// public class AggregatedContributionsResponse
// {
//     public string UserId { get; set; } = string.Empty;
//     public int Year { get; set; }
//     public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
//     public AggregatedTotals Totals { get; set; } = new();
//     public List<ProviderContribution> Providers { get; set; } = new();
    
//     public bool IncludeActivity { get; set; }
//     public bool IncludeBreakdown { get; set; }
// }

// public class AggregatedTotals
// {
//     public int TotalContributions { get; set; }
//     public int TotalCommits { get; set; }
//     public int TotalPullRequests { get; set; }
//     public int TotalWorkItems { get; set; }
// }

public class ProviderContribution
{
    public string Provider { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public ContributionsResponse? Data { get; set; }
}