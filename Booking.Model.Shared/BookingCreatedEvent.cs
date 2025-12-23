using SharedKernel.MessageBus.Abstraction;

namespace Booking.Model.Shared;

public record BookingCreatedEvent : IntegrationEvent
{
    public Guid Id { get; set; }
    public string SiteName { get; set; }
    public string PlateNumber { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BookingFrom { get; set; }
    public DateTime BookingTo { get; set; }
    public decimal TotalPrice { get; set; }
}