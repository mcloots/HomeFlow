using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Enums;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Households.Domain.Repositories;

namespace HomeFlow.Modules.Households.Application.Commands.InviteHouseholdMember;

public sealed class InviteHouseholdMemberHandler
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IHouseholdInvitationRepository _invitationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public InviteHouseholdMemberHandler(
        IHouseholdRepository householdRepository,
        IHouseholdInvitationRepository invitationRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _householdRepository = householdRepository;
        _invitationRepository = invitationRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<InviteHouseholdMemberResponse> Handle(
        InviteHouseholdMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var householdId = new HouseholdId(command.HouseholdId);

        var household = await _householdRepository.GetByIdAsync(householdId, cancellationToken);

        if (household is null)
            throw new InvalidOperationException("Household was not found.");

        if (!household.HasManagementPermissions(command.RequestedByEmail))
            throw new InvalidOperationException("You are not allowed to invite members to this household.");

        var invitedEmailNormalized = command.InvitedEmail.Trim().ToLowerInvariant();

        if (await _householdRepository.MemberEmailExistsAsync(invitedEmailNormalized, cancellationToken))
            throw new InvalidOperationException("A household member with this email already exists.");

        var pendingExists = await _invitationRepository.PendingInvitationExistsAsync(
            householdId,
            invitedEmailNormalized,
            cancellationToken);

        if (pendingExists)
            throw new InvalidOperationException("A pending invitation for this email already exists.");

        var invitationId = HouseholdInvitationId.New();

        var invitation = HouseholdInvitation.Create(
            invitationId,
            householdId,
            invitedEmailNormalized,
            command.Role,
            _dateTimeProvider.UtcNow);

        await _invitationRepository.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InviteHouseholdMemberResponse(
            invitationId.Value,
            householdId.Value,
            invitation.Email,
            invitation.Status.ToString());
    }
}