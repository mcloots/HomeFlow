using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;

public sealed class ScanGmailForAppointmentSuggestionsHandler
{
    private readonly IGmailConnectionRepository _connectionRepository;
    private readonly IGmailScanner _gmailScanner;

    public ScanGmailForAppointmentSuggestionsHandler(
        IGmailConnectionRepository connectionRepository,
        IGmailScanner gmailScanner)
    {
        _connectionRepository = connectionRepository;
        _gmailScanner = gmailScanner;
    }

    public async Task<ScanGmailForAppointmentSuggestionsResponse> Handle(
        ScanGmailForAppointmentSuggestionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ToUtc <= query.FromUtc)
            throw new InvalidOperationException("ToUtc must be after FromUtc.");

        var householdId = new HouseholdId(query.HouseholdId);

        var connection = await _connectionRepository.GetActiveByHouseholdIdAsync(
            householdId,
            cancellationToken);

        if (connection is null)
            throw new InvalidOperationException("No active Gmail connection was found for this household.");

        var suggestions = await _gmailScanner.ScanForAppointmentSuggestionsAsync(
            connection.EncryptedRefreshToken,
            query.FromUtc,
            query.ToUtc,
            cancellationToken);

        return new ScanGmailForAppointmentSuggestionsResponse(
            query.HouseholdId,
            query.FromUtc,
            query.ToUtc,
            suggestions);
    }
}