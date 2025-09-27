namespace Contribution.AzureDevOps.Models;

public class AzureApiResponse<T> where T : class
{
    public int Count { get; set; }
    public T? Value { get; set; }
}