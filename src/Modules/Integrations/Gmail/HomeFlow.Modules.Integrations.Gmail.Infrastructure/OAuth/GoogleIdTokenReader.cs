using Google.Apis.Auth;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.OAuth;

public sealed class GoogleIdTokenReader : IGoogleIdTokenReader
{
    public string ReadEmail(string idToken)
    {
        var payload = GoogleJsonWebSignature.ValidateAsync(idToken).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(payload.Email))
            throw new InvalidOperationException("Google ID token did not contain an email address.");

        return payload.Email;
    }
}