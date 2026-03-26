namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed record ScanGmailForAppointmentSuggestionsResponse(
    Guid HouseholdId,
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyCollection<AppointmentSuggestionDto> Suggestions);