namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed record ScanGmailForAppointmentSuggestionsQuery(
    Guid HouseholdId,
    DateTime FromUtc,
    DateTime ToUtc);