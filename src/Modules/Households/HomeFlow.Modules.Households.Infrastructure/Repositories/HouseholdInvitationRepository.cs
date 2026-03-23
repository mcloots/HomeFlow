using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Modules.Households.Infrastructure.Repositories;

public sealed class HouseholdInvitationRepository : IHouseholdInvitationRepository
{
    private readonly HomeFlowDbContext _dbContext;

    public HouseholdInvitationRepository(HomeFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HouseholdInvitation invitation, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<HouseholdInvitation>().AddAsync(invitation, cancellationToken);
    }

    public Task<HouseholdInvitation?> GetByIdAsync(
        HouseholdInvitationId invitationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<HouseholdInvitation>()
            .SingleOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
    }

    public Task<bool> PendingInvitationExistsAsync(
        HouseholdId householdId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Set<HouseholdInvitation>()
            .AnyAsync(x =>
                x.HouseholdId == householdId &&
                x.Email == normalizedEmail &&
                x.Status == InvitationStatus.Pending,
                cancellationToken);
    }
}