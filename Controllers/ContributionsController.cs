using AzureContributionsApi.Managers;
using AzureContributionsApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureContributionsApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ContributionsController(IContributionsManager svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string email,
        [FromQuery] string organization,
        [FromQuery] int year,
        [FromQuery] bool includeBreakdown = false)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(organization))
            return BadRequest("email and organization required");

        if (year < 1900 || year > 3000)
            return BadRequest("year must be a valid yyyy");

        (_, string pat) = AuthHelpers.ExtractAuthDetails(Request.Headers.Authorization.ToString() ?? string.Empty) ?? (string.Empty, string.Empty);

        var result = await svc.GetContributionsAsync(email, organization, year, pat, includeBreakdown);
        return Ok(result);
    }
}
