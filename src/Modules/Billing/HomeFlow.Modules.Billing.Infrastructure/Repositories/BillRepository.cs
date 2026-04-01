using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Billing.Domain.Aggregates;
using HomeFlow.Modules.Billing.Domain.Ids;
using HomeFlow.Modules.Billing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Billing.Infrastructure.Repositories;

public sealed class BillRepository : IBillRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public BillRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Bill>().AddAsync(bill, cancellationToken);
    }

    public Task<Bill?> GetByIdAsync(BillId billId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Bill>()
            .SingleOrDefaultAsync(x => x.Id == billId, cancellationToken);
    }
}
