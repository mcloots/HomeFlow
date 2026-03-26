using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace HomeFlow.Modules.Integrations.Gmail.Infrastructure.Security;

public sealed class DataProtectionTokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;

    public DataProtectionTokenEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("HomeFlow.GmailTokens.v1");
    }

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);

    public string Decrypt(string ciphertext) => _protector.Unprotect(ciphertext);
}