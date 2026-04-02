using HomeFlow.Modules.Chores.Domain.Aggregates;
using HomeFlow.Modules.Chores.Domain.Ids;

namespace HomeFlow.Modules.Chores.Domain.Repositories;

public interface IChoreRepository
{
    Task AddAsync(Chore chore, CancellationToken cancellationToken = default);
    Task<Chore?> GetByIdAsync(ChoreId choreId, CancellationToken cancellationToken = default);
}
