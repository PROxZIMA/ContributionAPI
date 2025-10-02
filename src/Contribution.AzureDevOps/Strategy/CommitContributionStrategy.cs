using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity;
using Contribution.Common.Models;
using Contribution.Common.Constants;
using Contribution.Common.Attributes;
using Contribution.AzureDevOps.Repository;
using Contribution.AzureDevOps.Factory;

namespace Contribution.AzureDevOps.Strategy;

[ContributionType(ContributionTypes.Commits)]
public sealed class CommitContributionStrategy(
    IAzureDevOpsRepository repository,
    IAzureClientFactory azureClientFactory,
    IOptions<ContributionsOptions> options,
    ILogger<CommitContributionStrategy> logger) : IContributionStrategy
{
    private readonly IAzureDevOpsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAzureClientFactory _azureClientFactory = azureClientFactory ?? throw new ArgumentNullException(nameof(azureClientFactory));
    private readonly IOptions<ContributionsOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<CommitContributionStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyDictionary<string, Common.Models.Contribution>> GetContributionsAsync(
        Identity userIdentity,
        string organization,
        string pat,
        DateTime fromDate,
        DateTime toDate)
    {
        var contributions = new Dictionary<string, int>();
        var projects = await _repository.GetProjectsAsync(organization, pat);
        var gitClient = await _azureClientFactory.GetGitClient(organization, pat);

        var throttler = new SemaphoreSlim(_options.Value.MaxConcurrency);
        var tasks = new List<Task>();

        foreach (var project in projects)
        {
            var repositories = await _repository.GetRepositoriesAsync(organization, pat, project);

            foreach (var repository in repositories)
            {
                await throttler.WaitAsync();
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessRepositoryCommitsAsync(
                            gitClient,
                            repository,
                            userIdentity,
                            fromDate,
                            toDate,
                            contributions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing commits for repository {Repository} in project {Project}",
                            repository.Name, project.Name);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                });
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
        return new ReadOnlyDictionary<string, Common.Models.Contribution>(
            contributions.ToDictionary(
                kvp => kvp.Key,
                kvp => new Common.Models.Contribution(kvp.Key, kvp.Value)));
    }

    private async Task ProcessRepositoryCommitsAsync(
        GitHttpClient gitClient,
        GitRepository repository,
        Identity userIdentity,
        DateTime fromDate,
        DateTime toDate,
        Dictionary<string, int> contributions)
    {
        var criteria = new GitQueryCommitsCriteria
        {
            FromDate = fromDate.ToString("o"),
            ToDate = toDate.ToString("o"),
            Author = userIdentity.Properties["Account"]?.ToString() ?? string.Empty,
            Committer = userIdentity.Properties["Account"]?.ToString() ?? string.Empty
        };

        int currentSkip = _options.Value.DefaultSkip;
        int pageTop = _options.Value.DefaultTop;

        while (true)
        {
            var commits = await gitClient.GetCommitsAsync(repository.Id, criteria, top: pageTop, skip: currentSkip);
            if (commits == null || commits.Count == 0) break;

            foreach (var commit in commits)
            {
                var commitDate = commit.Author?.Date ?? commit.Committer?.Date ?? DateTime.UtcNow;
                var utcDate = commitDate.ToUniversalTime();
                var dateKey = utcDate.ToString(SystemConstants.DateFormat);
                
                lock (contributions)
                {
                    contributions[dateKey] = contributions.TryGetValue(dateKey, out var value) ? value + 1 : 1;
                }
            }

            if (commits.Count < pageTop) break;
            currentSkip += pageTop;
        }
    }
}