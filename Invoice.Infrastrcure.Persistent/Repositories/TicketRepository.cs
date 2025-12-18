using System;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;

namespace Invoice.Infrastrcure.Persistent.Repositories;

public class TicketRepository : Repo<Ticket>
{
    public TicketRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
