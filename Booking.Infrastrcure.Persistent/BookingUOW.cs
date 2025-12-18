using System;
using Booking.Infrastrcure.Persistent.Repositories;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;

namespace Booking.Infrastrcure.Persistent;

public class BookingUOW : UOW
{
    private readonly DbContext _dbContext;
    IRepo<Ticket> Tickets { get; set; }
    IRepo<Site> Sites { get; set; }
    public BookingUOW(DbContext dbContext, IMessagePublisher messagePublisher, IIntegrationEventQueue messageQueue) : base(dbContext, messagePublisher, messageQueue)
    {
        _dbContext = dbContext;
        Tickets = new GenericRepo<Ticket>(_dbContext);
        Sites = new GenericRepo<Site>(_dbContext);
    }
}
