namespace Contribution.Common.Models;

public class ContributionsOptions
{
    public int MaxConcurrency { get; set; } = 8;
    public int ContributionsCacheMinutes { get; set; } = 1440;
    // Following 3 cache duration seems unnecessary as the api itself is cached for 1 day for improved performace (above)
    // TODO: revisit if needed
    public int ProjectsCacheMinutes { get; set; } = 15;
    public int IdentityCacheMinutes { get; set; } = 60;
    public int RepoCacheMinutes { get; set; } = 15;
    public int DefaultTop { get; set; } = 2000;
    public int DefaultSkip { get; set; } = 0;
}