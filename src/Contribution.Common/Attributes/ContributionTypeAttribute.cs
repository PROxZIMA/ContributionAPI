namespace Contribution.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ContributionTypeAttribute(string contributionType) : Attribute
{
    public string ContributionType { get; } = contributionType;
}