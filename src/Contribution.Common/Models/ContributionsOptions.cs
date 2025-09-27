namespace Contribution.Common.Models;

public class ContributionsOptions
{
    public int MaxConcurrency { get; set; } = 8;
    public int ProjectsCacheMinutes { get; set; } = 5;
    public int IdentityCacheMinutes { get; set; } = 60;
    public int RepoCacheMinutes { get; set; } = 5;
    public int DefaultTop { get; set; } = 2000;
    public int DefaultSkip { get; set; } = 0;
}