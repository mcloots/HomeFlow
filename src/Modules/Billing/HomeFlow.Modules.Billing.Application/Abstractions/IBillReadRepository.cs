using HomeFlow.Modules.Billing.Application.Queries.GetBillDetails;
using HomeFlow.Modules.Billing.Application.Queries.GetBillsForHousehold;

namespace HomeFlow.Modules.Billing.Application.Abstractions;

public interface IBillReadRepository
{
    Task<GetBillDetailsResponse?> GetDetailsByIdAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<GetBillsForHouseholdResponse> GetForHouseholdAsync(Guid householdId, CancellationToken cancellationToken = default);
}
