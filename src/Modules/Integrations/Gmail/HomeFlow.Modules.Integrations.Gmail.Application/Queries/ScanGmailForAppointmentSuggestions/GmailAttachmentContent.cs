namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed record GmailAttachmentContent(
    string FileName,
    string MimeType,
    string TextContent);
