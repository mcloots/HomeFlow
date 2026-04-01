namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed record AppointmentSuggestionDto(
    string SourceMessageId,
    DateTime SourceMessageDateUtc,
    string Sender,
    string Subject,
    string SourceSnippet,
    string SourceBody,
    IReadOnlyCollection<string> SourceAttachmentNames,
    string SourceAttachmentText,
    string SuggestedTitle,
    DateTime? SuggestedStartsAtUtc,
    DateTime? SuggestedEndsAtUtc,
    string? SuggestedLocation,
    string SuggestedType,
    string? SuggestedDescription,
    decimal? SuggestedAmount,
    decimal Confidence,
    string Reason);
