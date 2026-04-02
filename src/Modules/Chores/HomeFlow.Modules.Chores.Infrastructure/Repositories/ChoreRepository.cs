using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Chores.Domain.Aggregates;
using HomeFlow.Modules.Chores.Domain.Ids;
using HomeFlow.Modules.Chores.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Chores.Infrastructure.Repositories;

public sealed class ChoreRepository : IChoreRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public ChoreRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Chore chore, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Chore>().AddAsync(chore, cancellationToken);
    }

    public Task<Chore?> GetByIdAsync(ChoreId choreId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Chore>()
            .SingleOrDefaultAsync(x => x.Id == choreId, cancellationToken);
    }
}
