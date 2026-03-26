namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed record AppointmentSuggestionDto(
    string SourceMessageId,
    DateTime SourceMessageDateUtc,
    string Sender,
    string Subject,
    string SuggestedTitle,
    DateTime? SuggestedStartsAtUtc,
    DateTime? SuggestedEndsAtUtc,
    string? SuggestedLocation,
    string? SuggestedDescription,
    decimal Confidence,
    string Reason);