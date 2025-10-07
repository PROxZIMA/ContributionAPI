using Microsoft.AspNetCore.Mvc;
using Contribution.Hub.Managers;
using Contribution.Hub.Models;
using Contribution.Hub.Services;

namespace Contribution.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class ContributionsController(
    IContributionAggregatorManager aggregatorManager,
    ISvgGeneratorService svgGeneratorService,
    ILogger<ContributionsController> logger) : ControllerBase
{
    private readonly IContributionAggregatorManager _aggregatorManager = aggregatorManager;
    private readonly ISvgGeneratorService _svgGeneratorService = svgGeneratorService;
    private readonly ILogger<ContributionsController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string userId,
        [FromQuery] int year,
        [FromQuery] string[]? providers = null,
        [FromQuery] bool includeActivity = false,
        [FromQuery] bool includeBreakdown = false)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required");
        }

        if (year < 1900 || year > 3000)
        {
            return BadRequest("year must be a valid yyyy");
        }

        try
        {
            _logger.LogInformation("Fetching aggregated contributions for user {UserId}, year {Year}", userId, year);

            var result = await _aggregatorManager.GetAggregatedContributionsAsync(
                userId,
                year,
                providers,
                includeActivity,
                includeBreakdown);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch aggregated contributions for user {UserId}", userId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] UserData userData,
        [FromQuery] int year,
        [FromQuery] string[]? providers = null,
        [FromQuery] bool includeActivity = false,
        [FromQuery] bool includeBreakdown = false)
    {
        if (userData == null)
        {
            return BadRequest("Valid user data is required");
        }

        if (year < 1900 || year > 3000)
        {
            return BadRequest("year must be a valid yyyy");
        }

        try
        {
            _logger.LogInformation("Fetching aggregated contributions for user {UserId}, year {Year}", userData.Id, year);

            var result = await _aggregatorManager.GetAggregatedContributionsAsync(
                userData,
                year,
                providers,
                includeActivity,
                includeBreakdown);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for user {UserId}", userData.Id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch aggregated contributions for user {UserId}", userData.Id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("svg")]
    public async Task<IActionResult> GetSvg(
        [FromQuery] string userId,
        [FromQuery] int year,
        [FromQuery] string[]? providers = null,
        [FromQuery] int blockMargin = 4,
        [FromQuery] int blockRadius = 2,
        [FromQuery] int blockSize = 12,
        [FromQuery] bool darkMode = false,
        [FromQuery] int fontSize = 14,
        [FromQuery] bool hideColorLegend = false,
        [FromQuery] bool hideMonthLabels = false,
        [FromQuery] bool hideTotalCount = false,
        [FromQuery] int maxLevel = 4,
        [FromQuery] bool hideWeekdayLabels = false,
        [FromQuery] string? weekdayLabels = null,
        [FromQuery] int weekStart = 0,
        [FromQuery] bool showLoadingAnimation = true)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required");
        }

        if (year < 1900 || year > 3000)
        {
            return BadRequest("year must be a valid yyyy");
        }

        try
        {
            _logger.LogInformation("Generating SVG calendar for user {UserId}, year {Year}", userId, year);

            // Get aggregated contributions
            var contributionsResponse = await _aggregatorManager.GetAggregatedContributionsAsync(
                userId,
                year,
                providers,
                includeActivity: false,
                includeBreakdown: false);

            if (contributionsResponse.Contributions.Count == 0)
            {
                return NotFound("No contribution data found for the specified user and year");
            }

            // Create SVG options
            var svgOptions = new SvgOptions
            {
                BlockMargin = blockMargin,
                BlockRadius = blockRadius,
                BlockSize = blockSize,
                DarkMode = darkMode,
                FontSize = fontSize,
                HideColorLegend = hideColorLegend,
                HideMonthLabels = hideMonthLabels,
                HideTotalCount = hideTotalCount,
                MaxLevel = maxLevel,
                HideWeekdayLabels = hideWeekdayLabels,
                WeekdayLabels = weekdayLabels,
                TotalCount = contributionsResponse.Total.Sum(c => c.Value),
                WeekStart = weekStart,
                ShowLoadingAnimation = showLoadingAnimation
            };

            // Generate SVG
            var svg = _svgGeneratorService.GenerateActivityCalendarSvg(contributionsResponse.Contributions, svgOptions);
            
            // Set cache control headers to prevent caching
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            return Content(svg, "image/svg+xml");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for user {UserId}", userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SVG for user {UserId}", userId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost("svg")]
    public async Task<IActionResult> PostSvg(
        [FromBody] UserData userData,
        [FromQuery] int year,
        [FromQuery] string[]? providers = null,
        [FromQuery] int blockMargin = 4,
        [FromQuery] int blockRadius = 2,
        [FromQuery] int blockSize = 12,
        [FromQuery] bool darkMode = false,
        [FromQuery] int fontSize = 14,
        [FromQuery] bool hideColorLegend = false,
        [FromQuery] bool hideMonthLabels = false,
        [FromQuery] bool hideTotalCount = false,
        [FromQuery] int maxLevel = 4,
        [FromQuery] bool hideWeekdayLabels = false,
        [FromQuery] string? weekdayLabels = null,
        [FromQuery] int weekStart = 0,
        [FromQuery] bool showLoadingAnimation = true)
    {
        if (userData == null)
        {
            return BadRequest("Valid user data is required");
        }

        if (year < 1900 || year > 3000)
        {
            return BadRequest("year must be a valid yyyy");
        }

        try
        {
            _logger.LogInformation("Generating SVG calendar for user {UserId}, year {Year}", userData.Id, year);

            // Get aggregated contributions
            var contributionsResponse = await _aggregatorManager.GetAggregatedContributionsAsync(
                userData,
                year,
                providers,
                includeActivity: false,
                includeBreakdown: false);

            if (contributionsResponse.Contributions.Count == 0)
            {
                return NotFound("No contribution data found for the specified user and year");
            }

            // Create SVG options
            var svgOptions = new SvgOptions
            {
                BlockMargin = blockMargin,
                BlockRadius = blockRadius,
                BlockSize = blockSize,
                DarkMode = darkMode,
                FontSize = fontSize,
                HideColorLegend = hideColorLegend,
                HideMonthLabels = hideMonthLabels,
                HideTotalCount = hideTotalCount,
                MaxLevel = maxLevel,
                HideWeekdayLabels = hideWeekdayLabels,
                WeekdayLabels = weekdayLabels,
                TotalCount = contributionsResponse.Total.Sum(c => c.Value),
                WeekStart = weekStart,
                ShowLoadingAnimation = showLoadingAnimation
            };

            // Generate SVG
            var svg = _svgGeneratorService.GenerateActivityCalendarSvg(contributionsResponse.Contributions, svgOptions);
            
            // Set cache control headers to prevent caching
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            return Content(svg, "image/svg+xml");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for user {UserId}", userData.Id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SVG for user {UserId}", userData.Id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}