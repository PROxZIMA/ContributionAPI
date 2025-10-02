namespace Contribution.Common.Constants;

public static class SystemConstants
{
    public const string DateFormat = "yyyy-MM-dd";
}

public static class ContributionTypes
{
    // Breakdown/Activity keys for Azure DevOps
    public const string Commits = "commits";
    public const string WorkItems = "workitems";
    public const string PullRequests = "pullrequests";
    
    // Breakdown/Activity keys for GitHub
    public const string Issues = "issues";
    public const string Reviews = "reviews";
    public const string Restricted = "restricted";
    
    // General/Combined keys
    public const string All = "all";
}

public static class ProviderNames
{
    public const string Azure = "azure";
    public const string GitHub = "github";
}