using HomeFlow.Modules.Billing.Application.Abstractions;

namespace HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;

public sealed class GetBillsForHouseholdHandler
{
    private readonly IBillReadRepository _readRepository;

    public GetBillsForHouseholdHandler(IBillReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public Task<GetBillsForHouseholdResponse> Handle(
        GetBillsForHouseholdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _readRepository.GetForHouseholdAsync(query.HouseholdId, cancellationToken);
    }
}
