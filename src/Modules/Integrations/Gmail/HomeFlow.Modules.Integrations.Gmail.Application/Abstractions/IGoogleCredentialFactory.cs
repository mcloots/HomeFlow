using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGoogleCredentialFactory
{
    Task<IConfigurableHttpClientInitializer>
        CreateGmailReadonlyCredentialAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);
}