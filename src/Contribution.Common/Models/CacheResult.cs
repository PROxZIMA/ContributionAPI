namespace Contribution.Common.Models;

public class CacheResult<T>(T? value, bool isHit) where T : class
{
    public T? Value { get; set; } = value;
    public bool IsHit { get; set; } = isHit;
}