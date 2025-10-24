using Contribution.GitLab.Managers;
using Contribution.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contribution.GitLab.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ContributionsController(IGitLabContributionsManager svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string username,
        [FromQuery] int year,
        [FromQuery] bool includeActivity = false,
        [FromQuery] bool includeBreakdown = false)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("username required");

        if (year < 1900 || year > 3000)
            return BadRequest("year must be a valid yyyy");

        (_, string pat) = AuthHelpers.ExtractAuthDetails(Request.Headers.Authorization.ToString() ?? string.Empty) ?? (string.Empty, string.Empty);

        var result = await svc.GetContributionsAsync(username, year, pat, includeBreakdown, includeActivity);
        return Ok(result);
    }
}
