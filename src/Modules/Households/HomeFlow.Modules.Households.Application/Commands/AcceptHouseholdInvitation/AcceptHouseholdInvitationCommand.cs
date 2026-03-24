namespace HomeFlow.Modules.Households.Application.Commands.AcceptHouseholdInvitation;

public sealed record AcceptHouseholdInvitationCommand(
    Guid InvitationId,
    string DisplayName,
    string Email);