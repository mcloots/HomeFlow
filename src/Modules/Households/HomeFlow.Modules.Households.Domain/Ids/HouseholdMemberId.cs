namespace HomeFlow.Modules.Households.Domain.Ids;

public readonly record struct HouseholdMemberId(Guid Value)
{
    public static HouseholdMemberId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}