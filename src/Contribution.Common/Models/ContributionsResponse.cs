namespace Contribution.Common.Models;

public class ContributionsResponse
{
    public Dictionary<string,int> Total { get; set; } = [];
    public List<Contribution> Contributions { get; set; } = [];
    public Dictionary<string,int>? Breakdown { get; set; } = null;
    public MetaInfo Meta { get; set; } = new();
}

public class MetaInfo
{
    public int ScannedProjects { get; set; }
    public int ScannedRepos { get; set; }
    public long ElapsedMs { get; set; }
    public bool CachedProjects { get; set; }
    public List<string> Errors { get; set; } = [];
}