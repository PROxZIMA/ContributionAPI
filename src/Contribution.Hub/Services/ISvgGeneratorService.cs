using Contribution.Hub.Models;
using ContributionModel = Contribution.Common.Models.Contribution;

namespace Contribution.Hub.Services;

public interface ISvgGeneratorService
{
    string GenerateActivityCalendarSvg(List<ContributionModel> activities, SvgOptions options);
}
