using HomeFlow.Modules.Households.Application.Abstractions;

namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdMembers;

public sealed class GetHouseholdMembersHandler
{
    private readonly IHouseholdReadRepository _readRepository;

    public GetHouseholdMembersHandler(IHouseholdReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<GetHouseholdMembersResponse> Handle(
        GetHouseholdMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _readRepository.GetMembersByHouseholdIdAsync(
            query.HouseholdId,
            cancellationToken);

        if (result is null)
            throw new InvalidOperationException("Household was not found.");

        return result;
    }
}
