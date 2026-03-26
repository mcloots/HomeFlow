using HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGmailScanner
{
    Task<IReadOnlyCollection<AppointmentSuggestionDto>> ScanForAppointmentSuggestionsAsync(
        string encryptedRefreshToken,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}