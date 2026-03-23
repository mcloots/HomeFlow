using HomeFlow.BuildingBlocks.Application.Abstractions;

namespace HomeFlow.BuildingBlocks.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}