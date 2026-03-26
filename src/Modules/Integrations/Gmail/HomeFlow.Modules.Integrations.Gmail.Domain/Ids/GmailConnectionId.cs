namespace HomeFlow.Modules.Integrations.Gmail.Domain.Ids;

public readonly record struct GmailConnectionId(Guid Value)
{
    public static GmailConnectionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}