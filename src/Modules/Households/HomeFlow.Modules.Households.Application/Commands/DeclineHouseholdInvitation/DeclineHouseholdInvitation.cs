namespace HomeFlow.Modules.Households.Application.Commands.DeclineHouseholdInvitation;

public sealed record DeclineHouseholdInvitationCommand(
    Guid InvitationId,
    string Email);