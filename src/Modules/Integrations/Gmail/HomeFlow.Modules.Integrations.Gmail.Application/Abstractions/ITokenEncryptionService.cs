namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface ITokenEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}