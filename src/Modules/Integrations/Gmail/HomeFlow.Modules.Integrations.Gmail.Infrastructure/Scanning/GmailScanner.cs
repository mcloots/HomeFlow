using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

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
            var attachments = await ExtractAttachmentsAsync(service, message, cancellationToken);

            var suggestion = _parser.TryParse(message, attachments);

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

    private async Task<IReadOnlyCollection<GmailAttachmentContent>> ExtractAttachmentsAsync(
        GmailService service,
        Message message,
        CancellationToken cancellationToken)
    {
        if (message.Payload is null || string.IsNullOrWhiteSpace(message.Id))
            return [];

        var results = new List<GmailAttachmentContent>();
        await ExtractFromPartAsync(service, message.Id, message.Payload, results, cancellationToken);
        return results;
    }

    private async Task ExtractFromPartAsync(
        GmailService service,
        string messageId,
        MessagePart part,
        List<GmailAttachmentContent> results,
        CancellationToken cancellationToken)
    {
        if (IsSupportedAttachment(part))
        {
            var text = await ExtractAttachmentTextAsync(service, messageId, part, cancellationToken);

            if (!string.IsNullOrWhiteSpace(text))
            {
                results.Add(new GmailAttachmentContent(
                    part.Filename ?? "attachment",
                    part.MimeType ?? "application/octet-stream",
                    Truncate(text, 12000)));
            }
        }

        if (part.Parts is null)
            return;

        foreach (var subPart in part.Parts)
        {
            await ExtractFromPartAsync(service, messageId, subPart, results, cancellationToken);
        }
    }

    private async Task<string> ExtractAttachmentTextAsync(
        GmailService service,
        string messageId,
        MessagePart part,
        CancellationToken cancellationToken)
    {
        var bytes = await GetAttachmentBytesAsync(service, messageId, part, cancellationToken);

        if (bytes.Length == 0)
            return string.Empty;

        var mimeType = (part.MimeType ?? string.Empty).ToLowerInvariant();
        var fileName = (part.Filename ?? string.Empty).ToLowerInvariant();

        if (mimeType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.Ordinal))
            return ExtractPdfText(bytes);

        if (mimeType.Contains("html") || fileName.EndsWith(".html", StringComparison.Ordinal) || fileName.EndsWith(".htm", StringComparison.Ordinal))
            return StripHtml(Encoding.UTF8.GetString(bytes));

        if (mimeType.StartsWith("text/", StringComparison.Ordinal) ||
            fileName.EndsWith(".txt", StringComparison.Ordinal) ||
            fileName.EndsWith(".csv", StringComparison.Ordinal) ||
            fileName.EndsWith(".xml", StringComparison.Ordinal))
        {
            return Encoding.UTF8.GetString(bytes);
        }

        return string.Empty;
    }

    private async Task<byte[]> GetAttachmentBytesAsync(
        GmailService service,
        string messageId,
        MessagePart part,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(part.Body?.Data))
            return DecodeBase64Url(part.Body.Data);

        if (string.IsNullOrWhiteSpace(part.Body?.AttachmentId))
            return [];

        var request = service.Users.Messages.Attachments.Get("me", messageId, part.Body.AttachmentId);
        var attachment = await request.ExecuteAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(attachment.Data))
            return [];

        return DecodeBase64Url(attachment.Data);
    }

    private static bool IsSupportedAttachment(MessagePart part)
    {
        if (string.IsNullOrWhiteSpace(part.Filename))
            return false;

        var mimeType = (part.MimeType ?? string.Empty).ToLowerInvariant();
        var fileName = part.Filename.ToLowerInvariant();

        return mimeType.StartsWith("text/", StringComparison.Ordinal) ||
               mimeType.Contains("pdf") ||
               fileName.EndsWith(".pdf", StringComparison.Ordinal) ||
               fileName.EndsWith(".txt", StringComparison.Ordinal) ||
               fileName.EndsWith(".csv", StringComparison.Ordinal) ||
               fileName.EndsWith(".xml", StringComparison.Ordinal) ||
               fileName.EndsWith(".html", StringComparison.Ordinal) ||
               fileName.EndsWith(".htm", StringComparison.Ordinal);
    }

    private static string ExtractPdfText(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            using var document = PdfDocument.Open(stream);

            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return Regex.Replace(html, "<.*?>", " ");
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }

    private static byte[] DecodeBase64Url(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');

        switch (input.Length % 4)
        {
            case 2:
                input += "==";
                break;
            case 3:
                input += "=";
                break;
        }

        return Convert.FromBase64String(input);
    }
}
