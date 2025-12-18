using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using Site.Infrastrcure.Persistent.Repositories;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent;

public class SiteUOW : UOW
{
    private readonly DbContext _dbContext;
    IRepo<Model.Entities.Site> Sites { get; set; }
    IRepo<Polygon> Polygons { get; set; }
    IRepo<PolygonPoint> PolygonPoints { get; set; }
    public SiteUOW(DbContext dbContext, IMessagePublisher messagePublisher, IIntegrationEventQueue messageQueue) : base(dbContext, messagePublisher, messageQueue)
    {
        _dbContext = dbContext;

        Sites = new GenericRepo<Model.Entities.Site>(_dbContext);
        Polygons = new GenericRepo<Polygon>(_dbContext);
        PolygonPoints = new GenericRepo<PolygonPoint>(_dbContext);
    }
}
