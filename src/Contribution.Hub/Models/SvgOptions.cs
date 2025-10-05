namespace Contribution.Hub.Models;

/// <summary>
/// Configuration options for SVG activity calendar generation.
/// </summary>
public class SvgOptions
{
    /// <summary>
    /// Margin between calendar blocks in pixels. Default is 4.
    /// </summary>
    public int BlockMargin { get; set; } = 4;

    /// <summary>
    /// Border radius of calendar blocks in pixels. Default is 2.
    /// </summary>
    public int BlockRadius { get; set; } = 2;

    /// <summary>
    /// Size of calendar blocks in pixels. Default is 12.
    /// </summary>
    public int BlockSize { get; set; } = 12;

    /// <summary>
    /// Enable dark mode color scheme. False = light mode (default), true = dark mode.
    /// </summary>
    public bool DarkMode { get; set; } = false;

    /// <summary>
    /// Font size for text labels in pixels. Default is 14.
    /// </summary>
    public int FontSize { get; set; } = 14;

    /// <summary>
    /// Hide the color intensity legend below the calendar. Default is false.
    /// </summary>
    public bool HideColorLegend { get; set; } = false;

    /// <summary>
    /// Hide month labels above the calendar. Default is false.
    /// </summary>
    public bool HideMonthLabels { get; set; } = false;

    /// <summary>
    /// Hide the total contribution count below the calendar. Default is false.
    /// </summary>
    public bool HideTotalCount { get; set; } = false;

    /// <summary>
    /// Hide all weekday labels. Default is false.
    /// Use with WeekdayLabels to show specific days only.
    /// </summary>
    public bool HideWeekdayLabels { get; set; } = false;

    /// <summary>
    /// Comma-separated list of specific weekdays to show labels for.
    /// Examples: "mon,wed,fri" or "sun,sat" or "monday,wednesday,friday"
    /// If null and HideWeekdayLabels is false, shows every other day by default.
    /// </summary>
    public string? WeekdayLabels { get; set; }

    /// <summary>
    /// Custom labels for months, weekdays, and legend text.
    /// If null, default English labels are used.
    /// </summary>
    public SvgLabels? Labels { get; set; }

    /// <summary>
    /// Maximum activity level (zero-indexed). Default is 4.
    /// Level 0 = no activity, Level 4 = highest activity.
    /// </summary>
    public int MaxLevel { get; set; } = 4;

    /// <summary>
    /// Custom color theme for light and dark modes.
    /// Pass 2 colors (min/max) for gradient or 5 colors for full scale.
    /// If null, default GitHub-style colors are used.
    /// </summary>
    public SvgTheme? Theme { get; set; }

    /// <summary>
    /// Override the calculated total contribution count.
    /// If null, sum of all contribution counts is used.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// First day of the week. 0 = Sunday (default), 1 = Monday, etc.
    /// </summary>
    public int WeekStart { get; set; } = 0;

    /// <summary>
    /// Enable loading animation on calendar blocks. Default is false.
    /// When enabled, blocks fade in sequentially from left to right.
    /// </summary>
    public bool ShowLoadingAnimation { get; set; } = false;
}

/// <summary>
/// Custom labels for calendar text elements.
/// </summary>
public class SvgLabels
{
    /// <summary>
    /// Array of 12 month names. Default is ["Jan", "Feb", "Mar", ...].
    /// </summary>
    public string[]? Months { get; set; }

    /// <summary>
    /// Array of 7 weekday names starting with Sunday.
    /// Default is ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].
    /// </summary>
    public string[]? Weekdays { get; set; }

    /// <summary>
    /// Label for total count. Supports placeholders {{count}} and {{year}}.
    /// Default is "{{count}} contributions in {{year}}".
    /// </summary>
    public string? TotalCount { get; set; }

    /// <summary>
    /// Labels for the color legend (Less/More).
    /// </summary>
    public SvgLegendLabels? Legend { get; set; }
}

/// <summary>
/// Labels for the color intensity legend.
/// </summary>
public class SvgLegendLabels
{
    /// <summary>
    /// Label for lowest activity level. Default is "Less".
    /// </summary>
    public string Less { get; set; } = "Less";

    /// <summary>
    /// Label for highest activity level. Default is "More".
    /// </summary>
    public string More { get; set; } = "More";
}

/// <summary>
/// Custom color themes for light and dark modes.
/// </summary>
public class SvgTheme
{
    /// <summary>
    /// Colors for light mode. Pass 2 colors (min/max) for gradient 
    /// or 5 colors for full scale (matching MaxLevel + 1).
    /// Example: ["#ebedf0", "#9be9a8", "#40c463", "#30a14e", "#216e39"]
    /// </summary>
    public string[]? Light { get; set; }

    /// <summary>
    /// Colors for dark mode. Pass 2 colors (min/max) for gradient 
    /// or 5 colors for full scale (matching MaxLevel + 1).
    /// Example: ["#161b22", "#0e4429", "#006d32", "#26a641", "#39d353"]
    /// </summary>
    public string[]? Dark { get; set; }
}
