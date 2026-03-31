using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;
using HomeFlow.Modules.Integrations.Gmail.Application.Commands.StartGmailConnect;
using HomeFlow.Modules.Integrations.Gmail.Domain.Aggregates;
using HomeFlow.Modules.Integrations.Gmail.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Repositories;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.CompleteGmailConnect;

public sealed class CompleteGmailConnectHandler
{
    private readonly IGmailOAuthClient _gmailOAuthClient;
    private readonly IGmailOAuthStateStore _stateStore;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IGmailConnectionRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoogleIdTokenReader _googleIdTokenReader;

    public CompleteGmailConnectHandler(
        IGmailOAuthClient gmailOAuthClient,
        IGmailOAuthStateStore stateStore,
        ITokenEncryptionService tokenEncryptionService,
        IGmailConnectionRepository repository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IGoogleIdTokenReader googleIdTokenReader)
    {
        _gmailOAuthClient = gmailOAuthClient;
        _stateStore = stateStore;
        _tokenEncryptionService = tokenEncryptionService;
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _googleIdTokenReader = googleIdTokenReader;
    }

    public async Task<CompleteGmailConnectResponse> Handle(
        CompleteGmailConnectCommand command,
        CancellationToken cancellationToken = default)
    {
        var statePayload = _stateStore.Consume(command.State);

        if (statePayload is null)
            throw new InvalidOperationException("OAuth state is invalid or expired.");

        var existingConnection = await _repository.GetActiveByHouseholdIdAsync(
           statePayload.HouseholdId,
           cancellationToken);

        if (existingConnection is not null)
        {
            existingConnection.MarkDisconnected();
        }

        var tokenResult = await _gmailOAuthClient.ExchangeCodeAsync(
            command.Code,
            cancellationToken);

        if (!tokenResult.Scope.Contains("https://www.googleapis.com/auth/gmail.readonly", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google did not grant the required gmail.readonly scope.");
        }

        if (string.IsNullOrWhiteSpace(tokenResult.RefreshToken))
            throw new InvalidOperationException("No refresh token was returned by Google.");

        var googleEmail = _googleIdTokenReader.ReadEmail(tokenResult.IdToken);

        var connection = GmailConnection.Create(
            GmailConnectionId.New(),
            statePayload.TenantId,
            statePayload.HouseholdId,
            googleEmail,
            _tokenEncryptionService.Encrypt(tokenResult.RefreshToken),
            string.IsNullOrWhiteSpace(tokenResult.AccessToken)
                ? null
                : _tokenEncryptionService.Encrypt(tokenResult.AccessToken),
            tokenResult.Scope,
            _dateTimeProvider.UtcNow);

        await _repository.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompleteGmailConnectResponse(
            connection.Id.Value,
            connection.TenantId.Value,
            connection.HouseholdId.Value,
            connection.GoogleEmail,
            connection.Status.ToString());
    }
}