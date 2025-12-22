
using SharedKernel.MessageBus.Abstraction;
namespace Site.Model.Shared.Events;

public record SiteCreatedEvent : IntegrationEvent
{
    public Guid SiteId { get; init; }
    public string NameEn { get; init; }
    public string NameAr { get; init; }
    public string Path { get; init; }
    public bool IsLeaf { get; init; }
    public decimal? PricePerHour { get; init; }
    public string? IntegrationCode { get; init; }
    public int? NumberOfSolts { get; init; }
}