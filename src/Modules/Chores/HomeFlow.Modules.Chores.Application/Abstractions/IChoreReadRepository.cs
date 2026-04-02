using HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;

namespace HomeFlow.Modules.Chores.Application.Abstractions;

public interface IChoreReadRepository
{
    Task<GetChoresForHouseholdResponse> GetForHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);
}
