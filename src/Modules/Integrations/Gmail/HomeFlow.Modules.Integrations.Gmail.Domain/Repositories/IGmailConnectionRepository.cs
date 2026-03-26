using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Aggregates;
using HomeFlow.Modules.Integrations.Gmail.Domain.Ids;

namespace HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;

public interface IGmailConnectionRepository
{
    Task AddAsync(GmailConnection connection, CancellationToken cancellationToken = default);
    Task<GmailConnection?> GetByIdAsync(GmailConnectionId connectionId, CancellationToken cancellationToken = default);
    Task<GmailConnection?> GetActiveByHouseholdIdAsync(HouseholdId householdId, CancellationToken cancellationToken = default);
}