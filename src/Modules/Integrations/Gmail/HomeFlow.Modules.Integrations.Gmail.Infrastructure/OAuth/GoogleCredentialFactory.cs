using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using Google.Apis.Util.Store;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.OAuth;

public sealed class GoogleCredentialFactory : IGoogleCredentialFactory
{
    private const string GmailReadonlyScope =
        "https://www.googleapis.com/auth/gmail.readonly";

    private readonly GmailOAuthOptions _options;

    public GoogleCredentialFactory(IOptions<GmailOAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IConfigurableHttpClientInitializer>
        CreateGmailReadonlyCredentialAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new InvalidOperationException("Refresh token is required.");

        var flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret
                },
                Scopes = new[] { GmailReadonlyScope },

                // belangrijk: voorkomt dat Google zelf iets lokaal opslaat
                DataStore = new NullDataStore()
            });

        var token = new TokenResponse
        {
            RefreshToken = refreshToken
        };

        var userCredential = new UserCredential(
            flow,
            "gmail-scan",
            token);

        // Force token refresh nu
        await userCredential.GetAccessTokenForRequestAsync(
            cancellationToken: cancellationToken);

        return userCredential;
    }
}