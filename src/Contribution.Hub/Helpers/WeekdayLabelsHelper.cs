namespace Contribution.Hub.Helpers;

public class WeekdayLabelsConfig
{
    public bool ShouldShow { get; set; }
    private readonly bool[] _showByIndex = new bool[7];

    public bool ShouldShowForDayIndex(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex > 6)
            return false;
        return _showByIndex[dayIndex];
    }

    public void SetShowForDayIndex(int dayIndex, bool show)
    {
        if (dayIndex >= 0 && dayIndex <= 6)
            _showByIndex[dayIndex] = show;
    }

    public static WeekdayLabelsConfig Initialize(bool hideWeekdayLabels, string? weekdayLabels, int weekStart)
    {
        var config = new WeekdayLabelsConfig();

        if (hideWeekdayLabels)
        {
            config.ShouldShow = false;
            return config;
        }

        config.ShouldShow = true;

        // If specific weekday names are provided, use those
        if (!string.IsNullOrWhiteSpace(weekdayLabels))
        {
            var dayNames = weekdayLabels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var dayName in dayNames)
            {
                var index = DayNameToIndex(dayName?.ToLower());
                if (index.HasValue)
                {
                    config.SetShowForDayIndex(index.Value, true);
                }
            }
            return config;
        }

        // Default: Show every second day of the week (like React implementation)
        if (!hideWeekdayLabels)
        {
            for (int i = 0; i < 7; i++)
            {
                // Show labels for odd-positioned days relative to week start
                if ((7 + i - weekStart) % 7 % 2 != 0)
                {
                    config.SetShowForDayIndex(i, true);
                }
            }
        }

        return config;
    }

    private static int? DayNameToIndex(string? dayName)
    {
        return dayName switch
        {
            "sun" or "sunday" => 0,
            "mon" or "monday" => 1,
            "tue" or "tuesday" => 2,
            "wed" or "wednesday" => 3,
            "thu" or "thursday" => 4,
            "fri" or "friday" => 5,
            "sat" or "saturday" => 6,
            _ => null
        };
    }
}
