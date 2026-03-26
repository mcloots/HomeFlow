using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Application.Abstractions;

namespace HomeFlow.Modules.Integrations.Gmail.Application.Commands.StartGmailConnect;

public sealed class StartGmailConnectHandler
{
    private readonly IGmailOAuthClient _gmailOAuthClient;
    private readonly IGmailOAuthStateStore _stateStore;

    public StartGmailConnectHandler(
        IGmailOAuthClient gmailOAuthClient,
        IGmailOAuthStateStore stateStore)
    {
        _gmailOAuthClient = gmailOAuthClient;
        _stateStore = stateStore;
    }

    public Task<StartGmailConnectResponse> Handle(
        StartGmailConnectCommand command,
        CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var householdId = new HouseholdId(command.HouseholdId);

        var state = _stateStore.Create(tenantId, householdId);
        var authorizationUrl = _gmailOAuthClient.BuildAuthorizationUrl(state);

        return Task.FromResult(new StartGmailConnectResponse(authorizationUrl));
    }
}