using Google.Apis.Gmail.v1.Data;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Application.Queries.ScanGmailForAppointmentSuggestions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.Scanning;

public sealed class NaiveGmailSuggestionParser : IGmailSuggestionParser
{
    private static readonly Regex DateRegex =
        new(@"\b(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{4})\b", RegexOptions.Compiled);

    public AppointmentSuggestionDto? TryParse(Message message)
    {
        var headers = message.Payload?.Headers ?? [];
        var subject = GetHeader(headers, "Subject") ?? "(No subject)";
        var from = GetHeader(headers, "From") ?? "(Unknown sender)";
        var dateHeader = GetHeader(headers, "Date");

        var snippet = message.Snippet ?? string.Empty;
        var text = $"{subject}\n{snippet}";

        var suggestedDate = TryExtractDate(text);

        if (suggestedDate is null)
            return null;

        var sourceDate = TryParseHeaderDate(dateHeader) ?? DateTime.UtcNow;

        return new AppointmentSuggestionDto(
            message.Id ?? string.Empty,
            sourceDate,
            from,
            subject,
            subject,
            suggestedDate,
            suggestedDate.Value.AddHours(1),
            null,
            snippet,
            0.55m,
            "Detected date in subject/snippet");
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
}