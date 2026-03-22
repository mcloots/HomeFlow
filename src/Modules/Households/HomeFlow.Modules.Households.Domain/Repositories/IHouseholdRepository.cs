using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Households.Domain.Repositories;

public interface IHouseholdRepository
{
    Task AddAsync(Household household, CancellationToken cancellationToken = default);
    Task<Household?> GetByIdAsync(HouseholdId householdId, CancellationToken cancellationToken = default);
}