using Contribution.Hub.Models;
using ContributionModel = Contribution.Common.Models.Contribution;

namespace Contribution.Hub.Helpers;

public static class CalendarHelper
{
    private static readonly string[] DefaultMonthLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    private static readonly string[] DefaultWeekdayLabels = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    public static List<CalendarWeek> GroupByWeeks(List<ContributionModel> activities, int weekStart = 0)
    {
        if (activities.Count == 0)
            return [];

        var normalizedActivities = FillHoles(activities);
        var firstActivity = normalizedActivities[0];
        var firstDate = DateTime.Parse(firstActivity.Date);
        
        // Determine the first date of the calendar
        var firstDayOfWeek = (int)firstDate.DayOfWeek;
        var daysToSubtract = (firstDayOfWeek - weekStart + 7) % 7;
        var firstCalendarDate = firstDate.AddDays(-daysToSubtract);

        // Left-pad the list if needed
        var paddingDays = (int)(firstDate - firstCalendarDate).TotalDays;
        var paddedActivities = new List<ContributionModel?>(new ContributionModel?[paddingDays]);
        paddedActivities.AddRange(normalizedActivities);

        // Group by weeks
        var numberOfWeeks = (int)Math.Ceiling(paddedActivities.Count / 7.0);
        var weeks = new List<CalendarWeek>();

        for (int i = 0; i < numberOfWeeks; i++)
        {
            var week = new CalendarWeek();
            for (int j = 0; j < 7; j++)
            {
                var index = i * 7 + j;
                week.Days.Add(index < paddedActivities.Count ? paddedActivities[index] : null);
            }
            weeks.Add(week);
        }

        return weeks;
    }

    public static List<ContributionModel> FillHoles(List<ContributionModel> activities)
    {
        if (activities.Count == 0)
            return activities;

        var calendar = new Dictionary<string, ContributionModel>();
        foreach (var activity in activities)
        {
            calendar[activity.Date] = activity;
        }

        var firstDate = DateTime.Parse(activities[0].Date);
        var lastDate = DateTime.Parse(activities[^1].Date);
        var result = new List<ContributionModel>();

        for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            if (calendar.TryGetValue(dateStr, out var activity))
            {
                result.Add(activity);
            }
            else
            {
                result.Add(new ContributionModel(dateStr, 0));
            }
        }

        return result;
    }

    public static List<MonthLabel> GetMonthLabels(List<CalendarWeek> weeks, string[]? monthNames = null)
    {
        monthNames ??= DefaultMonthLabels;
        var labels = new List<MonthLabel>();

        for (int weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
        {
            var week = weeks[weekIndex];
            var firstActivity = week.Days.FirstOrDefault(d => d != null);
            
            if (firstActivity == null)
                continue;

            var date = DateTime.Parse(firstActivity.Date);
            var month = monthNames[date.Month - 1];

            var prevLabel = labels.Count > 0 ? labels[^1] : null;

            if (weekIndex == 0 || prevLabel == null || prevLabel.Label != month)
            {
                labels.Add(new MonthLabel { WeekIndex = weekIndex, Label = month });
            }
        }

        // Filter labels based on spacing
        return labels.Where((label, index) =>
        {
            const int minWeeks = 3;

            // Skip the first month label if there is not enough space to the next one
            if (index == 0)
            {
                return labels.Count > 1 && labels[1].WeekIndex - label.WeekIndex >= minWeeks;
            }

            // Skip the last month label if there is not enough data
            if (index == labels.Count - 1)
            {
                return weeks.Count - label.WeekIndex >= minWeeks;
            }

            return true;
        }).ToList();
    }

    public static string[] CreateColorScale(string[] colors, int steps)
    {
        if (colors.Length == steps)
            return colors;

        if (colors.Length != 2)
            throw new ArgumentException($"Color array must contain exactly 2 or {steps} colors");

        // For simplicity, create a basic gradient
        // In production, you might want a more sophisticated color interpolation
        var scale = new string[steps];
        scale[0] = colors[0];
        scale[steps - 1] = colors[1];

        // Simple linear interpolation for middle colors
        for (int i = 1; i < steps - 1; i++)
        {
            var ratio = (double)i / (steps - 1) * 100;
            scale[i] = $"color-mix(in oklab, {colors[1]} {ratio:F2}%, {colors[0]})";
        }

        return scale;
    }

    public static string[] GetColorScale(SvgTheme? theme, bool darkMode, int steps)
    {
        var defaultLight = new[] { "#ebedf0", "#9be9a8", "#40c463", "#30a14e", "#216e39" };
        var defaultDark = new[] { "#161b22", "#0e4429", "#006d32", "#26a641", "#39d353" };

        string[] colors;

        if (theme != null)
        {
            colors = darkMode 
                ? (theme.Dark ?? defaultDark)
                : (theme.Light ?? defaultLight);
        }
        else
        {
            colors = darkMode ? defaultDark : defaultLight;
        }

        return CreateColorScale(colors, steps);
    }

    public static string[] GetMonthLabels() => DefaultMonthLabels;
    public static string[] GetWeekdayLabels() => DefaultWeekdayLabels;
}
