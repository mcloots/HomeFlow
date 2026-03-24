using HomeFlow.Modules.Households.Application.Abstractions;

namespace HomeFlow.Modules.Households.Application.Queries.GetHouseholdDetails;

public sealed class GetHouseholdDetailsHandler
{
    private readonly IHouseholdReadRepository _readRepository;

    public GetHouseholdDetailsHandler(IHouseholdReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<GetHouseholdDetailsResponse> Handle(
        GetHouseholdDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _readRepository.GetDetailsByIdAsync(
            query.HouseholdId,
            cancellationToken);

        if (result is null)
            throw new InvalidOperationException("Household was not found.");

        return result;
    }
}