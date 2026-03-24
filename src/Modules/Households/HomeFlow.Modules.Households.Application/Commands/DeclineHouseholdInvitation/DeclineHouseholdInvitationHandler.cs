using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;

namespace HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;

public sealed class DeclineHouseholdInvitationHandler
{
    private readonly IHouseholdInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeclineHouseholdInvitationHandler(
        IHouseholdInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeclineHouseholdInvitationResponse> Handle(
        DeclineHouseholdInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var invitationId = new HouseholdInvitationId(command.InvitationId);

        var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);

        if (invitation is null)
            throw new InvalidOperationException("Invitation was not found.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (!string.Equals(invitation.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The provided email does not match the invitation email.");

        invitation.Decline();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeclineHouseholdInvitationResponse(
            invitation.Id.Value,
            invitation.Email,
            invitation.Status.ToString());
    }
}