using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;

namespace HomeFlow.Modules.Households.Application.Commands.RevokeHouseholdInvitation;

public sealed class RevokeHouseholdInvitationHandler
{
    private readonly IHouseholdInvitationRepository _invitationRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeHouseholdInvitationHandler(
        IHouseholdInvitationRepository invitationRepository,
        IHouseholdRepository householdRepository,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _householdRepository = householdRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RevokeHouseholdInvitationResponse> Handle(
        RevokeHouseholdInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var invitationId = new HouseholdInvitationId(command.InvitationId);

        var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);

        if (invitation is null)
            throw new InvalidOperationException("Invitation was not found.");

        var household = await _householdRepository.GetByIdAsync(invitation.HouseholdId, cancellationToken);

        if (household is null)
            throw new InvalidOperationException("Household was not found.");

        if (!household.HasManagementPermissions(command.RequestedByEmail))
            throw new InvalidOperationException("You are not allowed to revoke invitations for this household.");

        invitation.Revoke();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RevokeHouseholdInvitationResponse(
            invitation.Id.Value,
            invitation.HouseholdId.Value,
            invitation.Email,
            invitation.Status.ToString());
    }
}