using HomeFlow.Modules.Households.Domain.Ids;

namespace HomeFlow.Modules.Scheduling.Application.Abstractions;

public interface IHouseholdMemberLookup
{
    Task<bool> AllBelongToHouseholdAsync(
        HouseholdId householdId,
        IReadOnlyCollection<HouseholdMemberId> memberIds,
        CancellationToken cancellationToken = default);
}