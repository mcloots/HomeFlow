namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGmailOAuthClient
{
    string BuildAuthorizationUrl(string state);

    Task<GmailOAuthTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}

public sealed record GmailOAuthTokenResult(
    string AccessToken,
    string? RefreshToken,
    string Scope,
    string IdToken);