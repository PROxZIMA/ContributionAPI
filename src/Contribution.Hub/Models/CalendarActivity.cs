using ContributionModel = Contribution.Common.Models.Contribution;

namespace Contribution.Hub.Models;

public class CalendarWeek
{
    public List<ContributionModel?> Days { get; set; } = [];
}

public class MonthLabel
{
    public int WeekIndex { get; set; }
    public string Label { get; set; } = string.Empty;
}
