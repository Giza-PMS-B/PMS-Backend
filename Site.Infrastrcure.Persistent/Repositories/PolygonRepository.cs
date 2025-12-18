using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent.Repositories;

public class PolygonRepository : Repo<Polygon>
{
    public PolygonRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
