using SharedKernel.MessageBus.Abstraction;

namespace Site.Model.Shared;

public record SiteCreatedEvent : IntegrationEvent
{
    public required Guid SiteId { get; init; }
    public required string NameEn { get; init; }
    public required string NameAr { get; init; }
    public required string Path { get; init; }
    public required bool IsLeaf { get; init; }
    public decimal? PricePerHour { get; init; }
    public string? IntegrationCode { get; init; }
    public int? NumberOfSolts { get; init; }
}