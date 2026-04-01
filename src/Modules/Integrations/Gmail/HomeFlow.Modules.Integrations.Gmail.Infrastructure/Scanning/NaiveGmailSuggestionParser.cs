using Google.Apis.Gmail.v1.Data;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;
using HomeFlow.Modules.Scheduling.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.Scanning;

public sealed class NaiveGmailSuggestionParser : IGmailSuggestionParser
{
    private static readonly Regex DateRegex =
        new(@"\b(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{4})\b", RegexOptions.Compiled);
    private static readonly Regex AmountRegex =
        new(@"(?:EUR|€)\s*(\d+(?:[.,]\d{1,2})?)|(\d+(?:[.,]\d{1,2})?)\s*(?:EUR|€)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AppointmentSuggestionDto? TryParse(
        Message message,
        IReadOnlyCollection<GmailAttachmentContent> attachments)
    {
        var headers = message.Payload?.Headers ?? [];

        var subject = GetHeader(headers, "Subject") ?? "(No subject)";
        var from = GetHeader(headers, "From") ?? "(Unknown sender)";
        var dateHeader = GetHeader(headers, "Date");

        var snippet = message.Snippet ?? string.Empty;

        var bodyText = ExtractPlainTextBody(message);
        var normalizedBody = NormalizeWhitespace(bodyText);
        var attachmentNames = attachments
            .Select(x => x.FileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var attachmentText = NormalizeWhitespace(string.Join("\n", attachments.Select(x => x.TextContent)));

        var fullText =
            $"{subject}\n" +
            $"{snippet}\n" +
            $"{normalizedBody}\n" +
            $"{attachmentText}";

        var sourceDate =
            TryParseHeaderDate(dateHeader)
            ?? DateTime.UtcNow;

        var suggestedDate = TryExtractDate(fullText);
        var suggestedAmount = TryExtractAmount(fullText);

        if (LooksLikeInvoice(subject, from, fullText))
        {
            return BuildPaymentSuggestion(
                message,
                sourceDate,
                from,
                subject,
                snippet,
                normalizedBody,
                attachmentNames,
                attachmentText,
                suggestedDate,
                suggestedAmount);
        }

        if (suggestedDate is not null)
        {
            return new AppointmentSuggestionDto(
                message.Id ?? string.Empty,
                sourceDate,
                from,
                subject,
                snippet,
                normalizedBody,
                attachmentNames,
                attachmentText,
                BuildSuggestedTitle(subject, from),
                suggestedDate.Value,
                suggestedDate.Value.AddHours(1),
                null,
                AppointmentType.General.ToString(),
                snippet,
                suggestedAmount,
                0.75m,
                attachmentNames.Count > 0
                    ? "Detected date using email and attachment content"
                    : "Detected date in subject/snippet/body");
        }

        return null;
    }

    private static AppointmentSuggestionDto BuildPaymentSuggestion(
        Message message,
        DateTime sourceDate,
        string from,
        string subject,
        string snippet,
        string body,
        IReadOnlyCollection<string> attachmentNames,
        string attachmentText,
        DateTime? dueDate,
        decimal? suggestedAmount)
    {
        var fallbackDueDate =
            sourceDate.Date
                .AddDays(1)
                .AddHours(9);

        var paymentDueDate = dueDate ?? fallbackDueDate;
        var paymentStartDate = sourceDate.Date.AddHours(9);

        if (paymentDueDate <= paymentStartDate)
            paymentStartDate = paymentDueDate.AddMinutes(-15);

        return new AppointmentSuggestionDto(
            message.Id ?? string.Empty,
            sourceDate,
            from,
            subject,
            snippet,
            body,
            attachmentNames,
            attachmentText,
            BuildSuggestedTitle(subject, from),
            paymentStartDate,
            paymentDueDate,
            null,
            AppointmentType.Payment.ToString(),
            snippet,
            suggestedAmount,
            dueDate is null ? 0.40m : 0.80m,
            dueDate is null
                ? attachmentNames.Count > 0
                    ? "Invoice-like email detected with attachment content but without explicit due date"
                    : "Invoice-like email detected without explicit due date"
                : attachmentNames.Count > 0
                    ? "Invoice-like email detected with due date from email or attachment"
                    : "Invoice-like email detected with due date");
    }

    private static string? GetHeader(IList<MessagePartHeader> headers, string name)
    {
        return headers.FirstOrDefault(h =>
            string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static DateTime? TryExtractDate(string text)
    {
        var match = DateRegex.Match(text);
        if (!match.Success)
            return null;

        var day = int.Parse(match.Groups[1].Value);
        var month = int.Parse(match.Groups[2].Value);
        var year = int.Parse(match.Groups[3].Value);

        return new DateTime(year, month, day, 9, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime? TryParseHeaderDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            return dto.UtcDateTime;

        return null;
    }

    private static decimal? TryExtractAmount(string text)
    {
        var match = AmountRegex.Match(text);

        if (!match.Success)
            return null;

        var rawAmount = match.Groups[1].Success
            ? match.Groups[1].Value
            : match.Groups[2].Value;

        rawAmount = rawAmount.Replace(",", ".", StringComparison.Ordinal);

        if (decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            return amount;

        return null;
    }

    private static bool LooksLikeInvoice(string subject, string from, string snippet)
    {
        var text = $"{subject}\n{from}\n{snippet}".ToLowerInvariant();

        return text.Contains("factuur")
            || text.Contains("invoice")
            || text.Contains("betaling")
            || text.Contains("payment")
            || text.Contains("bill")
            || text.Contains("rekening")
            || text.Contains("vervaldatum")
            || text.Contains("amount due");
    }

    private static string BuildSuggestedTitle(string subject, string from)
    {
        if (subject.StartsWith("Uw factuur", StringComparison.OrdinalIgnoreCase) ||
            subject.StartsWith("Your invoice", StringComparison.OrdinalIgnoreCase))
        {
            return "Pay invoice";
        }

        if (subject.Contains("factuur", StringComparison.OrdinalIgnoreCase) ||
            subject.Contains("invoice", StringComparison.OrdinalIgnoreCase))
        {
            return $"Pay invoice - {subject}";
        }

        return subject;
    }

    private static string ExtractPlainTextBody(Message message)
    {
        if (message.Payload == null)
            return string.Empty;

        var plainText = FindPart(message.Payload, "text/plain");

        if (!string.IsNullOrWhiteSpace(plainText))
            return plainText;

        var htmlText = FindPart(message.Payload, "text/html");

        if (!string.IsNullOrWhiteSpace(htmlText))
            return StripHtml(htmlText);

        return string.Empty;
    }

    private static string FindPart(MessagePart part, string mimeType)
    {
        if (part.MimeType == mimeType && part.Body?.Data != null)
        {
            return DecodeBase64Url(part.Body.Data);
        }

        if (part.Parts == null)
            return string.Empty;

        foreach (var subPart in part.Parts)
        {
            var result = FindPart(subPart, mimeType);

            if (!string.IsNullOrWhiteSpace(result))
                return result;
        }

        return string.Empty;
    }

    private static string DecodeBase64Url(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.Replace('-', '+').Replace('_', '/');

        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }

        var bytes = Convert.FromBase64String(input);

        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return Regex.Replace(
            html,
            "<.*?>",
            " ");
    }

    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}
