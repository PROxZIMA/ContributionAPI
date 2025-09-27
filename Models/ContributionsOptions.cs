namespace AzureContributionsApi.Models;

public class ContributionsOptions
{
    public int MaxConcurrency { get; set; } = 8;
    public int ProjectsCacheMinutes { get; set; } = 5;
    public int IdentityCacheMinutes { get; set; } = 60;
    public int RepoCacheMinutes { get; set; } = 5;
    public int DefaultTop { get; set; } = 2000;
    public int DefaultSkip { get; set; } = 0;
}

public class Contribution(string Date, int Count)
{
    public string Date { get; set; } = Date;
    public int Count { get; set; } = Count;
    public int Level
    {
        get
        {
            if (Count <= 0) return 0;
            if (Count < 5) return 1;
            if (Count < 10) return 2;
            if (Count < 15) return 3;
            return 4;
        }
    }
    public Dictionary<string, int> Activity { get; set; } = [];
}

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
