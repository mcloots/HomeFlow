using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Chores.Domain.Enums;
using HomeFlow.Modules.Chores.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Chores.Domain.Aggregates;

public sealed class Chore : AggregateRoot<ChoreId>, ITenantOwned
{
    private Chore()
    {
    }

    public TenantId TenantId { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime DueDateUtc { get; private set; }
    public HouseholdMemberId? AssignedMemberId { get; private set; }
    public ChoreStatus Status { get; private set; }
    public ChoreRecurrence Recurrence { get; private set; }
    public int? RecurrenceMonths { get; private set; }
    public DateTime? RecursUntilUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public HouseholdMemberId? CompletedByMemberId { get; private set; }

    public static Chore Create(
        ChoreId id,
        TenantId tenantId,
        HouseholdId householdId,
        string title,
        string? description,
        DateTime dueDateUtc,
        HouseholdMemberId? assignedMemberId,
        ChoreRecurrence recurrence,
        int? recurrenceMonths)
    {
        ValidateState(tenantId, householdId, title, recurrence, recurrenceMonths);

        return new Chore
        {
            Id = id,
            TenantId = tenantId,
            HouseholdId = householdId,
            Title = title.Trim(),
            Description = NormalizeText(description),
            DueDateUtc = dueDateUtc,
            AssignedMemberId = assignedMemberId,
            Status = ChoreStatus.Pending,
            Recurrence = recurrence,
            RecurrenceMonths = NormalizeRecurrenceMonths(recurrence, recurrenceMonths),
            RecursUntilUtc = CalculateRecursUntilUtc(dueDateUtc, recurrence, recurrenceMonths)
        };
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateTime dueDateUtc,
        HouseholdMemberId? assignedMemberId,
        ChoreRecurrence recurrence,
        int? recurrenceMonths)
    {
        EnsurePending();
        ValidateState(TenantId, HouseholdId, title, recurrence, recurrenceMonths);

        Title = title.Trim();
        Description = NormalizeText(description);
        DueDateUtc = dueDateUtc;
        AssignedMemberId = assignedMemberId;
        Recurrence = recurrence;
        RecurrenceMonths = NormalizeRecurrenceMonths(recurrence, recurrenceMonths);
        RecursUntilUtc = CalculateRecursUntilUtc(dueDateUtc, recurrence, recurrenceMonths);
    }

    public void MarkCompleted(HouseholdMemberId completedByMemberId, DateTime completedAtUtc)
    {
        EnsurePending();

        CompletedByMemberId = completedByMemberId;
        CompletedAtUtc = completedAtUtc;
        Status = ChoreStatus.Completed;
    }

    public Chore? CreateNextOccurrence(ChoreId nextChoreId)
    {
        if (Status != ChoreStatus.Completed || Recurrence == ChoreRecurrence.None)
            return null;

        var nextDueDateUtc = GetNextDueDateUtc(DueDateUtc, Recurrence);

        if (RecursUntilUtc.HasValue && nextDueDateUtc > RecursUntilUtc.Value)
            return null;

        return Create(
            nextChoreId,
            TenantId,
            HouseholdId,
            Title,
            Description,
            nextDueDateUtc,
            AssignedMemberId,
            Recurrence,
            RecurrenceMonths);
    }

    private static void ValidateState(
        TenantId tenantId,
        HouseholdId householdId,
        string title,
        ChoreRecurrence recurrence,
        int? recurrenceMonths)
    {
        if (tenantId == default)
            throw new DomainException("TenantId is required.");

        if (householdId == default)
            throw new DomainException("HouseholdId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (!Enum.IsDefined(recurrence))
            throw new DomainException("Recurrence is invalid.");

        if (recurrence == ChoreRecurrence.None && recurrenceMonths.HasValue)
            throw new DomainException("Recurrence months can only be set for recurring chores.");

        if (recurrence != ChoreRecurrence.None && (!recurrenceMonths.HasValue || recurrenceMonths.Value <= 0))
            throw new DomainException("Recurring chores must define a duration in months.");
    }

    private void EnsurePending()
    {
        if (Status == ChoreStatus.Completed)
            throw new DomainException("Completed chores cannot be modified.");
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime GetNextDueDateUtc(DateTime currentDueDateUtc, ChoreRecurrence recurrence)
    {
        return recurrence switch
        {
            ChoreRecurrence.Daily => currentDueDateUtc.AddDays(1),
            ChoreRecurrence.Weekly => currentDueDateUtc.AddDays(7),
            ChoreRecurrence.Monthly => currentDueDateUtc.AddMonths(1),
            _ => currentDueDateUtc
        };
    }

    private static int? NormalizeRecurrenceMonths(ChoreRecurrence recurrence, int? recurrenceMonths)
    {
        return recurrence == ChoreRecurrence.None ? null : recurrenceMonths;
    }

    private static DateTime? CalculateRecursUntilUtc(
        DateTime dueDateUtc,
        ChoreRecurrence recurrence,
        int? recurrenceMonths)
    {
        if (recurrence == ChoreRecurrence.None || !recurrenceMonths.HasValue)
            return null;

        return dueDateUtc.AddMonths(recurrenceMonths.Value);
    }
}
