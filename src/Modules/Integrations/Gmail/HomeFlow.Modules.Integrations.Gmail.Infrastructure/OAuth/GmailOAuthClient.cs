using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.OAuth;

public sealed class GmailOAuthClient : IGmailOAuthClient
{
    private readonly GmailOAuthOptions _options;
    private const string GmailReadonlyScope = "https://www.googleapis.com/auth/gmail.readonly";
    private const string OpenIdScope = "openid";
    private const string EmailScope = "email";

    public GmailOAuthClient(IOptions<GmailOAuthOptions> options)
    {
        _options = options.Value;
    }

    public string BuildAuthorizationUrl(string state)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = new[] { GmailReadonlyScope, OpenIdScope, EmailScope }
        });

        var request = flow.CreateAuthorizationCodeRequest(_options.RedirectUri);
        if (request is GoogleAuthorizationCodeRequestUrl googleRequest)
        {
            googleRequest.State = state;
            googleRequest.AccessType = "offline";
            googleRequest.IncludeGrantedScopes = "true";
            googleRequest.Prompt = "consent";
        }
        else
        {
            // fallback, zou normaal niet nodig zijn
            request.State = state;
        }

        return request.Build().ToString();
    }

    public async Task<GmailOAuthTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var flow = CreateFlow();

        var token = await flow.ExchangeCodeForTokenAsync(
            userId: "gmail-connect",
            code: code,
            redirectUri: _options.RedirectUri,
            taskCancellationToken: cancellationToken);

        return new GmailOAuthTokenResult(
            token.AccessToken,
            token.RefreshToken,
            token.Scope ?? string.Empty,
            token.IdToken);
    }

    public async Task RevokeTokenAsync(
     string refreshToken,
     CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = refreshToken
        });

        using var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/revoke",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private GoogleAuthorizationCodeFlow CreateFlow()
    {
        return new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret
                },
                Scopes = new[] { GmailReadonlyScope, OpenIdScope, EmailScope }
            });
    }
}