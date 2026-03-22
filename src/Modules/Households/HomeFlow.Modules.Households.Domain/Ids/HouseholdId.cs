namespace HomeFlow.Modules.Households.Domain.Ids;

public readonly record struct HouseholdId(Guid Value)
{
    public static HouseholdId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}