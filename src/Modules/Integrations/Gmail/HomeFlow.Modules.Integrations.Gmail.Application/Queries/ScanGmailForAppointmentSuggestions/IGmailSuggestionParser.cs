using Google.Apis.Gmail.v1.Data;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGmailSuggestionParser
{
    AppointmentSuggestionDto? TryParse(
        Message message,
        IReadOnlyCollection<GmailAttachmentContent> attachments);
}
