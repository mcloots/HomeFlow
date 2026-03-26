using HomeFlow.BuildingBlocks.Domain.Common;
using HomeFlow.BuildingBlocks.Domain.Exceptions;
using HomeFlow.BuildingBlocks.MultiTenancy.Abstractions;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Integrations.Gmail.Domain.Enums;
using HomeFlow.Modules.Integrations.Gmail.Domain.Ids;

namespace HomeFlow.Modules.Integrations.Gmail.Domain.Aggregates;

public sealed class GmailConnection : AggregateRoot<GmailConnectionId>, ITenantOwned
{
    private GmailConnection()
    {
    }

    public TenantId TenantId { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string GoogleEmail { get; private set; } = default!;
    public string EncryptedRefreshToken { get; private set; } = default!;
    public string? EncryptedAccessToken { get; private set; }
    public string Scopes { get; private set; } = default!;
    public GmailConnectionStatus Status { get; private set; }
    public DateTime ConnectedAtUtc { get; private set; }

    public static GmailConnection Create(
        GmailConnectionId id,
        TenantId tenantId,
        HouseholdId householdId,
        string googleEmail,
        string encryptedRefreshToken,
        string? encryptedAccessToken,
        string scopes,
        DateTime connectedAtUtc)
    {
        if (tenantId == default)
            throw new DomainException("TenantId is required.");

        if (householdId == default)
            throw new DomainException("HouseholdId is required.");

        if (string.IsNullOrWhiteSpace(googleEmail))
            throw new DomainException("Google email is required.");

        if (string.IsNullOrWhiteSpace(encryptedRefreshToken))
            throw new DomainException("Refresh token is required.");

        if (string.IsNullOrWhiteSpace(scopes))
            throw new DomainException("Scopes are required.");

        return new GmailConnection
        {
            Id = id,
            TenantId = tenantId,
            HouseholdId = householdId,
            GoogleEmail = googleEmail.Trim().ToLowerInvariant(),
            EncryptedRefreshToken = encryptedRefreshToken,
            EncryptedAccessToken = encryptedAccessToken,
            Scopes = scopes,
            Status = GmailConnectionStatus.Active,
            ConnectedAtUtc = connectedAtUtc
        };
    }

    public void MarkDisconnected()
    {
        if (Status != GmailConnectionStatus.Active)
            throw new DomainException("Only active Gmail connections can be disconnected.");

        Status = GmailConnectionStatus.Disconnected;
    }

    public void MarkRevoked()
    {
        Status = GmailConnectionStatus.Revoked;
    }
}