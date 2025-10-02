using Microsoft.AspNetCore.Mvc;
using Contribution.Hub.Managers;

namespace Contribution.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class ContributionsController(
    IContributionAggregatorManager aggregatorManager,
    ILogger<ContributionsController> logger) : ControllerBase
{
    private readonly IContributionAggregatorManager _aggregatorManager = aggregatorManager;
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
}