using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Identity;
using AzureContributionsApi.Models;
using AzureContributionsApi.Repository;
using AzureContributionsApi.Factory;
using AzureContributionsApi.Constants;

namespace AzureContributionsApi.Strategy;

public sealed class WorkItemContributionStrategy(
    IAzureDevOpsRepository repository,
    IAzureClientFactory azureClientFactory,
    IOptions<ContributionsOptions> options,
    ILogger<WorkItemContributionStrategy> logger) : IContributionStrategy
{
    private readonly IAzureDevOpsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAzureClientFactory _azureClientFactory = azureClientFactory ?? throw new ArgumentNullException(nameof(azureClientFactory));
    private readonly IOptions<ContributionsOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<WorkItemContributionStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyDictionary<string, Contribution>> GetContributionsAsync(
        Identity userIdentity,
        string organization,
        string pat,
        DateTime fromDate,
        DateTime toDate)
    {
        var contributions = new Dictionary<string, int>();
        var projects = await _repository.GetProjectsAsync(organization, pat);
        var workItemClient = await _azureClientFactory.GetWorkItemClient(organization, pat);
        
        var throttler = new SemaphoreSlim(_options.Value.MaxConcurrency);
        var tasks = new List<Task>();

        foreach (var project in projects)
        {
            await throttler.WaitAsync();
            var task = Task.Run(async () =>
            {
                try
                {
                    await ProcessProjectWorkItemsAsync(
                        workItemClient,
                        project.Id,
                        userIdentity,
                        fromDate,
                        toDate,
                        contributions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing work items for project {Project}", project.Name);
                }
                finally
                {
                    throttler.Release();
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        return new ReadOnlyDictionary<string, Contribution>(
            contributions.ToDictionary(
                kvp => kvp.Key,
                kvp => new Contribution(kvp.Key, kvp.Value)));
    }

    private static async Task ProcessProjectWorkItemsAsync(
        WorkItemTrackingHttpClient workItemClient,
        Guid projectId,
        Identity userIdentity,
        DateTime fromDate,
        DateTime toDate,
        Dictionary<string, int> contributions)
    {
        var userEmail = userIdentity.Properties["Account"]?.ToString() ?? string.Empty;
        
        // WIQL query to find work items created or modified by the user
        var wiql = new Wiql
        {
            Query = $@"
                SELECT [System.Id], [System.CreatedDate], [System.ChangedDate] 
                FROM WorkItems 
                WHERE ([System.CreatedBy] = '{userEmail}' OR [System.ChangedBy] = '{userEmail}') 
                AND [System.ChangedDate] >= '{fromDate:yyyy-MM-dd}' 
                AND [System.ChangedDate] <= '{toDate:yyyy-MM-dd}'
                ORDER BY [System.ChangedDate] DESC"
        };

        var queryResult = await workItemClient.QueryByWiqlAsync(wiql, projectId);
        
        if (queryResult?.WorkItems == null || !queryResult.WorkItems.Any()) 
            return;

        // Get work item details in batches
        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToArray();
        const int batchSize = 200; // Azure DevOps API limit
        
        for (int i = 0; i < workItemIds.Length; i += batchSize)
        {
            var batch = workItemIds.Skip(i).Take(batchSize).ToArray();
            var workItems = await workItemClient.GetWorkItemsAsync(
                batch, 
                fields: ["System.CreatedDate", "System.ChangedDate", "System.CreatedBy", "System.ChangedBy"]);

            foreach (var workItem in workItems)
            {
                // Count work item creation
                if (workItem.Fields.TryGetValue("System.CreatedDate", out var createdDateValue) &&
                    workItem.Fields.TryGetValue("System.CreatedBy", out var createdByValue) &&
                    DateTime.TryParse(createdDateValue?.ToString(), out var createdDate) &&
                    createdByValue?.ToString()?.Contains(userEmail, StringComparison.OrdinalIgnoreCase) == true &&
                    createdDate >= fromDate && createdDate <= toDate)
                {
                    var dateKey = createdDate.ToString(SystemConstants.DateFormat);
                    lock (contributions)
                    {
                        contributions[dateKey] = contributions.TryGetValue(dateKey, out var value) ? value + 1 : 1;
                    }
                }

                // Count work item updates (only if not the same day as creation)
                if (workItem.Fields.TryGetValue("System.ChangedDate", out var changedDateValue) &&
                    workItem.Fields.TryGetValue("System.ChangedBy", out var changedByValue) &&
                    DateTime.TryParse(changedDateValue?.ToString(), out var changedDate) &&
                    changedByValue?.ToString()?.Contains(userEmail, StringComparison.OrdinalIgnoreCase) == true &&
                    changedDate >= fromDate && changedDate <= toDate)
                {
                    var changedDateKey = changedDate.ToString(SystemConstants.DateFormat);
                    var createdDateKey = workItem.Fields.TryGetValue("System.CreatedDate", out var createdVal) &&
                                       DateTime.TryParse(createdVal?.ToString(), out var created) 
                                       ? created.ToString(SystemConstants.DateFormat) 
                                       : string.Empty;

                    // Only count if it's a different day than creation to avoid double counting
                    if (changedDateKey != createdDateKey)
                    {
                        lock (contributions)
                        {
                            contributions[changedDateKey] = contributions.TryGetValue(changedDateKey, out var value) ? value + 1 : 1;
                        }
                    }
                }
            }
        }
    }
}