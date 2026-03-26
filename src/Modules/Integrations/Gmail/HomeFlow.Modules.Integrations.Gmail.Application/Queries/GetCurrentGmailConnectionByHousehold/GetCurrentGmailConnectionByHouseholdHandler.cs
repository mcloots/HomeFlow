using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.GetCurrentGmailConnectionByHousehold;

public sealed class GetCurrentGmailConnectionByHouseholdHandler
{
    private readonly IGmailConnectionRepository _repository;

    public GetCurrentGmailConnectionByHouseholdHandler(
        IGmailConnectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetCurrentGmailConnectionByHouseholdResponse?> Handle(
        GetCurrentGmailConnectionByHouseholdQuery query,
        CancellationToken cancellationToken = default)
    {
        var householdId = new HouseholdId(query.HouseholdId);

        var connection = await _repository.GetActiveByHouseholdIdAsync(
            householdId,
            cancellationToken);

        if (connection is null)
            return null;

        return new GetCurrentGmailConnectionByHouseholdResponse(
            connection.Id.Value,
            connection.TenantId.Value,
            connection.HouseholdId.Value,
            connection.GoogleEmail,
            connection.Status.ToString(),
            connection.ConnectedAtUtc);
    }
}