using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Households.Domain.Repositories;

public interface IHouseholdInvitationRepository
{
    Task AddAsync(HouseholdInvitation invitation, CancellationToken cancellationToken = default);
    Task<HouseholdInvitation?> GetByIdAsync(HouseholdInvitationId invitationId, CancellationToken cancellationToken = default);
    Task<bool> PendingInvitationExistsAsync(
        HouseholdId householdId,
        string email,
        CancellationToken cancellationToken = default);
}