using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.DisconnectGmailConnection;

public sealed class DisconnectGmailConnectionHandler
{
    private readonly IGmailConnectionRepository _repository;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IGmailOAuthClient _gmailOAuthClient;
    private readonly IUnitOfWork _unitOfWork;

    public DisconnectGmailConnectionHandler(
        IGmailConnectionRepository repository,
        ITokenEncryptionService tokenEncryptionService,
        IGmailOAuthClient gmailOAuthClient,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tokenEncryptionService = tokenEncryptionService;
        _gmailOAuthClient = gmailOAuthClient;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DisconnectGmailConnectionCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = new GmailConnectionId(command.GmailConnectionId);

        var connection = await _repository.GetByIdAsync(id, cancellationToken);

        if (connection is null)
            throw new InvalidOperationException("Gmail connection was not found.");

        var refreshToken = _tokenEncryptionService.Decrypt(connection.EncryptedRefreshToken);

        await _gmailOAuthClient.RevokeTokenAsync(refreshToken, cancellationToken);

        connection.MarkDisconnected();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}