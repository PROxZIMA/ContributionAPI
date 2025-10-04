using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Identity;
using Contribution.Common.Models;
using Contribution.Common.Constants;
using Contribution.Common.Attributes;
using Contribution.AzureDevOps.Repository;
using Contribution.AzureDevOps.Factory;
using System.Globalization;

namespace Contribution.AzureDevOps.Strategy;

[ContributionType(ContributionTypes.WorkItems)]
public sealed class WorkItemContributionStrategy(
    IAzureDevOpsRepository repository,
    IAzureClientFactory azureClientFactory,
    IOptions<ContributionsOptions> options,
    ILogger<WorkItemContributionStrategy> logger) : IContributionStrategy
{
    private readonly IAzureDevOpsRepository _repository = repository;
    private readonly IAzureClientFactory _azureClientFactory = azureClientFactory;
    private readonly IOptions<ContributionsOptions> _options = options;
    private readonly ILogger<WorkItemContributionStrategy> _logger = logger;

    public async Task<IReadOnlyDictionary<string, Common.Models.Contribution>> GetContributionsAsync(
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
        return new ReadOnlyDictionary<string, Common.Models.Contribution>(
            contributions.ToDictionary(
                kvp => kvp.Key,
                kvp => new Common.Models.Contribution(kvp.Key, kvp.Value)));
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
        var userId = userIdentity.Id.ToString();
        var fromDate_ = fromDate.ToString(SystemConstants.UTCDateTimeFormat, CultureInfo.InvariantCulture);
        var toDate_ = toDate.ToString(SystemConstants.UTCDateTimeFormat, CultureInfo.InvariantCulture);

        // WIQL query to find work items created or modified by the user
        var wiql = new Wiql
        {
            Query = $@"
                SELECT [System.Id] 
                FROM WorkItems 
                WHERE (
                    ([System.CreatedBy] = '{userEmail}' AND [System.CreatedDate] >= '{fromDate_}' AND [System.CreatedDate] <= '{toDate_}')
                    OR 
                    ([System.ChangedBy] = '{userEmail}' AND [System.ChangedDate] >= '{fromDate_}' AND [System.ChangedDate] <= '{toDate_}')
                )
                ORDER BY [System.ChangedDate] DESC"
        };

        var queryResult = await workItemClient.QueryByWiqlAsync(wiql, projectId);

        if (queryResult?.WorkItems == null || !queryResult.WorkItems.Any())
            return;

        // Get work item details in batches
        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).Distinct().ToArray();
        const int batchSize = 200;

        var requiredFields = new[] { "System.CreatedDate", "System.ChangedDate", "System.CreatedBy", "System.ChangedBy" };

        foreach (var batch in workItemIds.Chunk(batchSize))
        {
            var workItems = await workItemClient.GetWorkItemsAsync(batch, fields: requiredFields);

            foreach (var workItem in workItems)
            {
                ProcessWorkItemContributions(workItem, userId, fromDate, toDate, contributions);
            }
        }
    }

    private static void ProcessWorkItemContributions(
        WorkItem workItem,
        string userId,
        DateTime fromDate,
        DateTime toDate,
        Dictionary<string, int> contributions)
    {
        var createdDate = GetDateFromField(workItem, "System.CreatedDate");
        var changedDate = GetDateFromField(workItem, "System.ChangedDate");
        var createdBy = GetIdentityFromField(workItem, "System.CreatedBy");
        var changedBy = GetIdentityFromField(workItem, "System.ChangedBy");

        // Process creation contribution
        if (createdDate.HasValue && 
            IsWithinDateRange(createdDate.Value, fromDate, toDate) &&
            IsUserMatch(createdBy, userId))
        {
            var dateKey = createdDate.Value.ToString(SystemConstants.DateFormat);
            IncrementContribution(contributions, dateKey);
        }

        // Process change contribution (avoid double counting same-day creation/change)
        if (changedDate.HasValue && 
            IsWithinDateRange(changedDate.Value, fromDate, toDate) &&
            IsUserMatch(changedBy, userId) &&
            !IsSameDayAsCreation(createdDate, changedDate.Value))
        {
            var dateKey = changedDate.Value.ToString(SystemConstants.DateFormat);
            IncrementContribution(contributions, dateKey);
        }
    }

    private static DateTime? GetDateFromField(WorkItem workItem, string fieldName)
    {
        return workItem.Fields.TryGetValue(fieldName, out var value) && value is DateTime dateValue 
            ? dateValue 
            : null;
    }

    private static string? GetIdentityFromField(WorkItem workItem, string fieldName)
    {
        return workItem.Fields.TryGetValue(fieldName, out var value) && value is Microsoft.VisualStudio.Services.WebApi.IdentityRef identityRef
            ? identityRef.Id
            : null;
    }

    private static bool IsWithinDateRange(DateTime date, DateTime fromDate, DateTime toDate) =>
        date >= fromDate && date <= toDate;

    private static bool IsUserMatch(string? fieldUserId, string targetUserId) =>
        !string.IsNullOrEmpty(fieldUserId) && 
        string.Equals(fieldUserId, targetUserId, StringComparison.OrdinalIgnoreCase);

    private static bool IsSameDayAsCreation(DateTime? createdDate, DateTime changedDate)
    {
        if (!createdDate.HasValue) return false;
        
        var createdDateKey = createdDate.Value.ToString(SystemConstants.DateFormat);
        var changedDateKey = changedDate.ToString(SystemConstants.DateFormat);
        return string.Equals(createdDateKey, changedDateKey, StringComparison.OrdinalIgnoreCase);
    }

    private static void IncrementContribution(Dictionary<string, int> contributions, string dateKey)
    {
        lock (contributions)
        {
            contributions[dateKey] = contributions.TryGetValue(dateKey, out var existingValue) ? existingValue + 1 : 1;
        }
    }
}