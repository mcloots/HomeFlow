namespace HomeFlow.Modules.Chores.Domain.Ids;

public readonly record struct ChoreId(Guid Value)
{
    public static ChoreId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
