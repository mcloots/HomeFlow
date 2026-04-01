using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Billing.Domain.Enums;
using HomeFlow.Modules.Billing.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Billing.Domain.Aggregates;

public sealed class Bill : AggregateRoot<BillId>, ITenantOwned
{
    private Bill()
    {
    }

    public TenantId TenantId { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string Title { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public DateTime DueDateUtc { get; private set; }
    public BillStatus Status { get; private set; }
    public string? Category { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    public static Bill Create(
        BillId id,
        TenantId tenantId,
        HouseholdId householdId,
        string title,
        decimal amount,
        DateTime dueDateUtc,
        string? category)
    {
        if (tenantId == default)
            throw new DomainException("TenantId is required.");

        if (householdId == default)
            throw new DomainException("HouseholdId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        return new Bill
        {
            Id = id,
            TenantId = tenantId,
            HouseholdId = householdId,
            Title = title.Trim(),
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            DueDateUtc = dueDateUtc,
            Status = BillStatus.Pending,
            Category = NormalizeCategory(category)
        };
    }

    public void UpdateDetails(
        string title,
        decimal amount,
        DateTime dueDateUtc,
        string? category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        Title = title.Trim();
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        DueDateUtc = dueDateUtc;
        Category = NormalizeCategory(category);
    }

    public void SetStatus(BillStatus status, DateTime? paidAtUtc = null)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException("Bill status is invalid.");

        switch (status)
        {
            case BillStatus.Pending:
            case BillStatus.Overdue:
                Status = status;
                PaidAtUtc = null;
                break;
            case BillStatus.Paid:
                Status = BillStatus.Paid;
                PaidAtUtc = paidAtUtc ?? DateTime.UtcNow;
                break;
        }
    }

    private static string? NormalizeCategory(string? category)
    {
        return string.IsNullOrWhiteSpace(category) ? null : category.Trim();
    }
}
