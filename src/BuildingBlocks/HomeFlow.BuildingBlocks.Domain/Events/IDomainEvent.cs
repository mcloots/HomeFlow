namespace HomeFlow.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}