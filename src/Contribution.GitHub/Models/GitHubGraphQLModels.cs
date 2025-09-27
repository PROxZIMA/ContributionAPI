using System.Text.Json.Serialization;

namespace Contribution.GitHub.Models;

public class GitHubGraphQLResponse<T>
{
    [JsonPropertyName("data")] public T? Data { get; set; }
    [JsonPropertyName("errors")] public List<GitHubGraphQLError>? Errors { get; set; }
}

public class GitHubGraphQLError
{
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("path")] public List<string>? Path { get; set; }
    [JsonPropertyName("locations")] public List<GitHubGraphQLErrorLocation>? Locations { get; set; }
}

public class GitHubGraphQLErrorLocation
{
    [JsonPropertyName("line")] public int Line { get; set; }
    [JsonPropertyName("column")] public int Column { get; set; }
}

public class GitHubUserRoot
{
    [JsonPropertyName("user")] public GitHubUser? User { get; set; }
}

public class GitHubUser
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("login")] public string? Login { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("contributionsCollection")] public GitHubContributionsCollection? ContributionsCollection { get; set; }
}

public class GitHubContributionsCollection
{
    [JsonPropertyName("totalCommitContributions")] public int TotalCommitContributions { get; set; }
    [JsonPropertyName("totalIssueContributions")] public int TotalIssueContributions { get; set; }
    [JsonPropertyName("totalPullRequestContributions")] public int TotalPullRequestContributions { get; set; }
    [JsonPropertyName("totalPullRequestReviewContributions")] public int TotalPullRequestReviewContributions { get; set; }
    [JsonPropertyName("totalRepositoryContributions")] public int TotalRepositoryContributions { get; set; }
    [JsonPropertyName("restrictedContributionsCount")] public int RestrictedContributionsCount { get; set; }
    [JsonPropertyName("contributionCalendar")] public GitHubContributionCalendar? ContributionCalendar { get; set; }
}

public class GitHubContributionCalendar
{
    [JsonPropertyName("totalContributions")] public int TotalContributions { get; set; }
    [JsonPropertyName("weeks")] public List<GitHubContributionWeek> Weeks { get; set; } = [];
}

public class GitHubContributionWeek
{
    [JsonPropertyName("contributionDays")] public List<GitHubContributionDay> ContributionDays { get; set; } = [];
}

public class GitHubContributionDay
{
    [JsonPropertyName("date")] public DateTime Date { get; set; }
    [JsonPropertyName("contributionCount")] public int ContributionCount { get; set; }
    [JsonPropertyName("contributionLevel")] public string? ContributionLevel { get; set; }
}
