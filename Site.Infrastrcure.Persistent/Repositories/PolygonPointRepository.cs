using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent.Repositories;

public class PolygonPointRepository : Repo<PolygonPoint>
{
    public PolygonPointRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
