using System.Text;
using Contribution.Hub.Models;
using Contribution.Hub.Helpers;
using ContributionModel = Contribution.Common.Models.Contribution;

namespace Contribution.Hub.Services;

public class SvgGeneratorService : ISvgGeneratorService
{
    private const int LabelMargin = 8;
    private const string Namespace = "contribution-calendar";

    public string GenerateActivityCalendarSvg(List<ContributionModel> activities, SvgOptions options)
    {
        if (activities.Count == 0)
            throw new ArgumentException("Activity data must not be empty");

        var maxLevel = Math.Max(1, options.MaxLevel);
        var colorScale = CalendarHelper.GetColorScale(options.Theme, options.DarkMode, maxLevel + 1);
        var weeks = CalendarHelper.GroupByWeeks(activities, options.WeekStart);
        
        var labels = GetLabels(options.Labels);
        var labelHeight = options.HideMonthLabels ? 0 : options.FontSize + LabelMargin;
        
        var weekdayLabelsConfig = WeekdayLabelsConfig.Initialize(
            options.HideWeekdayLabels, 
            options.WeekdayLabels, 
            options.WeekStart);
        var weekdayLabelOffset = weekdayLabelsConfig.ShouldShow ? GetWeekdayLabelWidth(options.FontSize) : 0;

        var dimensions = GetDimensions(weeks, options, labelHeight);
        var svg = new StringBuilder();

        // Start SVG with embedded styles
        svg.AppendLine($"<svg width=\"{dimensions.Width + weekdayLabelOffset}\" height=\"{dimensions.Height}\" viewBox=\"0 0 {dimensions.Width + weekdayLabelOffset} {dimensions.Height}\" xmlns=\"http://www.w3.org/2000/svg\" class=\"{Namespace}\">");
        svg.AppendLine(GenerateStyles(options.DarkMode, options.FontSize, options.ShowLoadingAnimation));
        
        // Main calendar group
        svg.AppendLine($"<g transform=\"translate({weekdayLabelOffset}, 0)\">");

        // Render month labels
        if (!options.HideMonthLabels)
        {
            svg.AppendLine(RenderMonthLabels(weeks, labels, options));
        }

        // Render weekday labels
        if (weekdayLabelsConfig.ShouldShow)
        {
            svg.AppendLine(RenderWeekdayLabels(labels, options, labelHeight, weekdayLabelsConfig));
        }

        // Render calendar blocks
        svg.AppendLine(RenderCalendar(weeks, colorScale, options, labelHeight));

        svg.AppendLine("</g>");

        // Render footer (legend and total count)
        if (!options.HideColorLegend || !options.HideTotalCount)
        {
            var year = DateTime.Parse(activities[0].Date).Year;
            svg.AppendLine(RenderFooter(colorScale, labels, options, dimensions, weekdayLabelOffset, year));
        }

        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private static (int Width, int Height) GetDimensions(List<CalendarWeek> weeks, SvgOptions options, int labelHeight)
    {
        var width = weeks.Count * (options.BlockSize + options.BlockMargin) - options.BlockMargin;
        var baseHeight = labelHeight + (options.BlockSize + options.BlockMargin) * 7 - options.BlockMargin;

        // Add extra height for footer if either legend or total count is shown
        var footerHeight = (!options.HideColorLegend || !options.HideTotalCount) ? (options.FontSize + LabelMargin) : 0;

        var height = baseHeight + footerHeight;
        return (width, height);
    }

    private static string GenerateStyles(bool darkMode, int fontSize, bool showLoadingAnimation)
    {
        var strokeColor = darkMode 
            ? "rgba(255, 255, 255, 0.04)"
            : "rgba(0, 0, 0, 0.08)";

        var textColor = darkMode ? "#ffffff" : "#000000";

        var animationStyles = showLoadingAnimation ? $@"
    @keyframes fadeIn {{
        from {{
            opacity: 0;
        }}
        to {{
            opacity: 1;
        }}
    }}
    .{Namespace}__calendar-day {{
        animation: fadeIn 0.3s ease-in;
        animation-fill-mode: backwards;
    }}" : "";

        return $@"<style>
    .{Namespace} text {{
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
        font-size: {fontSize}px;
        fill: {textColor};
    }}
    .{Namespace} rect {{
        stroke: {strokeColor};
        stroke-width: 1px;
    }}{animationStyles}
</style>";
    }

    private static SvgLabels GetLabels(SvgLabels? customLabels)
    {
        var defaultLabels = new SvgLabels
        {
            Months = CalendarHelper.GetMonthLabels(),
            Weekdays = CalendarHelper.GetWeekdayLabels(),
            TotalCount = "{{count}} contributions in {{year}}",
            Legend = new SvgLegendLabels { Less = "Less", More = "More" }
        };

        if (customLabels == null)
            return defaultLabels;

        return new SvgLabels
        {
            Months = customLabels.Months ?? defaultLabels.Months,
            Weekdays = customLabels.Weekdays ?? defaultLabels.Weekdays,
            TotalCount = customLabels.TotalCount ?? defaultLabels.TotalCount,
            Legend = customLabels.Legend ?? defaultLabels.Legend
        };
    }

    private static int GetWeekdayLabelWidth(int fontSize)
    {
        // Approximate width - in production, you might want more precise calculation
        return (int)(fontSize * 3.5) + LabelMargin;
    }

    private static string RenderMonthLabels(List<CalendarWeek> weeks, SvgLabels labels, SvgOptions options)
    {
        var monthLabels = CalendarHelper.GetMonthLabels(weeks, labels.Months);
        var svg = new StringBuilder();

        svg.AppendLine($"<g class=\"{Namespace}__month-labels\">");
        foreach (var monthLabel in monthLabels)
        {
            var x = (options.BlockSize + options.BlockMargin) * monthLabel.WeekIndex;
            svg.AppendLine($"  <text x=\"{x}\" y=\"{0}\" dominant-baseline=\"hanging\">{monthLabel.Label}</text>");
        }
        svg.AppendLine("</g>");

        return svg.ToString();
    }

    private static string RenderWeekdayLabels(SvgLabels labels, SvgOptions options, int labelHeight, WeekdayLabelsConfig weekdayConfig)
    {
        var svg = new StringBuilder();
        svg.AppendLine($"<g class=\"{Namespace}__weekday-labels\">");

        for (int i = 0; i < 7; i++)
        {
            var dayIndex = (i + options.WeekStart) % 7;
            
            // Check if this specific day should show a label
            if (!weekdayConfig.ShouldShowForDayIndex(dayIndex))
                continue;

            var y = labelHeight + (options.BlockSize + options.BlockMargin) * i + options.BlockSize / 2.0;
            svg.AppendLine($"  <text x=\"{-LabelMargin}\" y=\"{y}\" text-anchor=\"end\" dominant-baseline=\"central\">{labels.Weekdays![dayIndex]}</text>");
        }

        svg.AppendLine("</g>");
        return svg.ToString();
    }

    private static string RenderCalendar(List<CalendarWeek> weeks, string[] colorScale, SvgOptions options, int labelHeight)
    {
        var svg = new StringBuilder();

        for (int weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
        {
            var week = weeks[weekIndex];
            var translateX = (options.BlockSize + options.BlockMargin) * weekIndex;
            svg.AppendLine($"<g class=\"{Namespace}__calendar-week\" transform=\"translate({translateX}, 0)\">");

            for (int dayIndex = 0; dayIndex < week.Days.Count; dayIndex++)
            {
                var contribution = week.Days[dayIndex];
                if (contribution == null)
                    continue;

                var y = labelHeight + (options.BlockSize + options.BlockMargin) * dayIndex;
                var color = colorScale[contribution.Level];

                // Calculate animation delay for sequential loading effect
                var animationDelay = options.ShowLoadingAnimation 
                    ? $" style=\"animation-delay: {weekIndex * 0.01}s\"" 
                    : "";

                svg.AppendLine($"  <rect class=\"{Namespace}__calendar-day\" x=\"0\" y=\"{y}\" width=\"{options.BlockSize}\" height=\"{options.BlockSize}\" " +
                              $"rx=\"{options.BlockRadius}\" ry=\"{options.BlockRadius}\" " +
                              $"fill=\"{color}\" data-date=\"{contribution.Date}\" data-level=\"{contribution.Level}\" data-count=\"{contribution.Count}\"{animationDelay}>");
                svg.AppendLine($"    <title>{contribution.Count} activities on {contribution.Date}</title>");
                svg.AppendLine("  </rect>");
            }

            svg.AppendLine("</g>");
        }

        return svg.ToString();
    }

    private static string RenderFooter(string[] colorScale, SvgLabels labels, SvgOptions options, 
        (int Width, int Height) dimensions, int weekdayLabelOffset, int year)
    {
        var svg = new StringBuilder();
        var footerY = dimensions.Height - options.FontSize;

        svg.AppendLine($"<g class=\"{Namespace}__footer\" transform=\"translate({weekdayLabelOffset}, {footerY})\">");

        // Total count on the left
        if (!options.HideTotalCount)
        {
            var countText = labels.TotalCount!
                .Replace("{{count}}", options.TotalCount.ToString())
                .Replace("{{year}}", year.ToString());
            svg.AppendLine($"  <text class=\"{Namespace}__count\" x=\"0\" y=\"0\" dominant-baseline=\"hanging\">{countText}</text>");
        }

        // Color legend on the right, aligned horizontally with total count
        if (!options.HideColorLegend)
        {
            var lessLegendWidth = 30;
            // "Less" text + blocks + "More" text
            var legendWidth = lessLegendWidth + (colorScale.Length * (options.BlockSize + options.BlockMargin)) + 40;
            // Position legend to the right of the calendar width
            var legendX = dimensions.Width - legendWidth;
            
            svg.AppendLine($"  <g class=\"{Namespace}__legend\" transform=\"translate({legendX}, 0)\">"); // TODO: was -5
            
            var currentX = 0;
            svg.AppendLine($"    <text x=\"{currentX}\" y=\"{options.BlockSize / 2}\" dominant-baseline=\"central\">{labels.Legend!.Less}</text>");
            currentX += lessLegendWidth;

            for (int level = 0; level < colorScale.Length; level++)
            {
                svg.AppendLine($"    <rect class=\"{Namespace}__legend-level\" x=\"{currentX}\" y=\"0\" width=\"{options.BlockSize}\" height=\"{options.BlockSize}\" " +
                              $"rx=\"{options.BlockRadius}\" ry=\"{options.BlockRadius}\" fill=\"{colorScale[level]}\">");
                svg.AppendLine($"      <title>Level: {level}</title>");
                svg.AppendLine("    </rect>");
                currentX += options.BlockSize + options.BlockMargin;
            }

            svg.AppendLine($"    <text x=\"{currentX}\" y=\"{options.BlockSize / 2}\" dominant-baseline=\"central\">{labels.Legend!.More}</text>");
            svg.AppendLine("  </g>");
        }

        svg.AppendLine("</g>");

        return svg.ToString();
    }
}
