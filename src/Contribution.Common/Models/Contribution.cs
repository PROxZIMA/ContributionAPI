namespace Contribution.Common.Models;

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