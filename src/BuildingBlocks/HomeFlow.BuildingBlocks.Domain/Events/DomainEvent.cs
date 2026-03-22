namespace HomeFlow.BuildingBlocks.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}