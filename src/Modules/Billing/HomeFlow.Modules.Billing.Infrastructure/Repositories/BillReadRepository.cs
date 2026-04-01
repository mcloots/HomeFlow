using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Billing.Application.Abstractions;
using HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;
using HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;
using HomeFlow.Modules.Billing.Domain.Aggregates;
using HomeFlow.Modules.Billing.Domain.Enums;
using HomeFlow.Modules.Billing.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Ids;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Billing.Infrastructure.Repositories;

public sealed class BillReadRepository : IBillReadRepository
{
    private readonly HomeFlowDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BillReadRepository(
        HomeFlowDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<GetBillDetailsResponse?> GetDetailsByIdAsync(
        Guid billId,
        CancellationToken cancellationToken = default)
    {
        var typedBillId = new BillId(billId);

        var bill = await _dbContext.Set<Bill>()
            .SingleOrDefaultAsync(x => x.Id == typedBillId, cancellationToken);

        if (bill is null)
            return null;

        return new GetBillDetailsResponse(
            bill.Id.Value,
            bill.TenantId.Value,
            bill.HouseholdId.Value,
            bill.Title,
            bill.Amount,
            bill.DueDateUtc,
            ResolveStatus(bill).ToString(),
            bill.Category,
            bill.PaidAtUtc);
    }

    public async Task<GetBillsForHouseholdResponse> GetForHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var typedHouseholdId = new HouseholdId(householdId);

        var bills = await _dbContext.Set<Bill>()
            .Where(x => x.HouseholdId == typedHouseholdId)
            .OrderBy(x => x.DueDateUtc)
            .ToListAsync(cancellationToken);

        var billDtos = bills
            .Select(x => new BillSummaryDto(
                x.Id.Value,
                x.Title,
                x.Amount,
                x.DueDateUtc,
                ResolveStatus(x).ToString(),
                x.Category,
                x.PaidAtUtc))
            .ToList();

        return new GetBillsForHouseholdResponse(householdId, billDtos);
    }

    private BillStatus ResolveStatus(Bill bill)
    {
        if (bill.Status == BillStatus.Paid)
            return BillStatus.Paid;

        return bill.DueDateUtc < _dateTimeProvider.UtcNow
            ? BillStatus.Overdue
            : BillStatus.Pending;
    }
}
