using HomeFlow.Modules.Chores.Application.Abstractions;

namespace HomeFlow.Modules.Chores.Application.Queries.GetChoresForHousehold;

public sealed class GetChoresForHouseholdHandler
{
    private readonly IChoreReadRepository _choreReadRepository;

    public GetChoresForHouseholdHandler(IChoreReadRepository choreReadRepository)
    {
        _choreReadRepository = choreReadRepository;
    }

    public Task<GetChoresForHouseholdResponse> Handle(
        GetChoresForHouseholdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _choreReadRepository.GetForHouseholdAsync(query.HouseholdId, cancellationToken);
    }
}
