namespace HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

public interface IGoogleIdTokenReader
{
    string ReadEmail(string idToken);
}