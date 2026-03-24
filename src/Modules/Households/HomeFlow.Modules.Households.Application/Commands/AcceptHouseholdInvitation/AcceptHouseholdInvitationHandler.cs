using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;

namespace HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;

public sealed class AcceptHouseholdInvitationHandler
{
    private readonly IHouseholdInvitationRepository _invitationRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptHouseholdInvitationHandler(
        IHouseholdInvitationRepository invitationRepository,
        IHouseholdRepository householdRepository,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _householdRepository = householdRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AcceptHouseholdInvitationResponse> Handle(
        AcceptHouseholdInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var invitationId = new HouseholdInvitationId(command.InvitationId);

        var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);

        if (invitation is null)
            throw new InvalidOperationException("Invitation was not found.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (!string.Equals(invitation.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The provided email does not match the invitation email.");

        var household = await _householdRepository.GetByIdAsync(invitation.HouseholdId, cancellationToken);

        if (household is null)
            throw new InvalidOperationException("Household was not found.");

        var existingMember = await _householdRepository.MemberEmailExistsAsync(normalizedEmail, cancellationToken);

        if (existingMember)
            throw new InvalidOperationException("A household member with this email already exists.");

        var memberId = HouseholdMemberId.New();

        household.AddMember(
            memberId,
            command.DisplayName,
            normalizedEmail,
            invitation.Role);

        invitation.Accept();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AcceptHouseholdInvitationResponse(
            invitation.Id.Value,
            invitation.HouseholdId.Value,
            memberId.Value,
            normalizedEmail,
            invitation.Status.ToString());
    }
}