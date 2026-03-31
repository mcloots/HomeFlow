using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.Scanning;

public sealed class GmailScanner : IGmailScanner
{
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IGmailSuggestionParser _parser;
    private readonly IGoogleCredentialFactory _credentialFactory;

    public GmailScanner(
        ITokenEncryptionService tokenEncryptionService,
        IGmailSuggestionParser parser,
        IGoogleCredentialFactory credentialFactory)
    {
        _tokenEncryptionService = tokenEncryptionService;
        _parser = parser;
        _credentialFactory = credentialFactory;
    }

    public async Task<IReadOnlyCollection<Application.Queries.ScanGmailForAppointmentSuggestions.AppointmentSuggestionDto>>
        ScanForAppointmentSuggestionsAsync(
            string encryptedRefreshToken,
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
    {
        var refreshToken = _tokenEncryptionService.Decrypt(encryptedRefreshToken);

        var service = await CreateGmailServiceAsync(refreshToken, cancellationToken);

        var q = BuildQuery(fromUtc, toUtc);

        var listRequest = service.Users.Messages.List("me");
        listRequest.Q = q;
        listRequest.MaxResults = 500;

        var listResponse = await listRequest.ExecuteAsync(cancellationToken);

        if (listResponse.Messages is null || listResponse.Messages.Count == 0)
            return [];

        var results = new List<Application.Queries.ScanGmailForAppointmentSuggestions.AppointmentSuggestionDto>();

        foreach (var item in listResponse.Messages)
        {
            var getRequest = service.Users.Messages.Get("me", item.Id);
            getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;

            var message = await getRequest.ExecuteAsync(cancellationToken);

            var suggestion = _parser.TryParse(message);

            if (suggestion is not null)
                results.Add(suggestion);
        }

        return results;
    }

    private static string BuildQuery(DateTime fromUtc, DateTime toUtc)
    {
        var afterDate = fromUtc.Date.ToString("yyyy/MM/dd");
        var beforeDate = toUtc.Date.AddDays(1).ToString("yyyy/MM/dd");

        var keywords =
            "(subject:invoice OR subject:factuur OR subject:betaling OR subject:bill OR subject:rekening OR invoice OR factuur OR betaling OR bill OR rekening)";

        var query =
            $"after:{afterDate} " +
            $"before:{beforeDate} " +
            $"{keywords}";

        return query;
    }

    private async Task<GmailService> CreateGmailServiceAsync(
    string refreshToken,
    CancellationToken cancellationToken)
    {
        var initializer =
            await _credentialFactory.CreateGmailReadonlyCredentialAsync(
                refreshToken,
                cancellationToken);

        return new GmailService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = initializer,
                ApplicationName = "HomeFlow"
            });
    }
}