using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;

namespace Site.Infrastrcure.Persistent.Repositories;

public class SiteRepository : Repo<Model.Entities.Site>
{
    public SiteRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
