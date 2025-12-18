using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;

namespace Invoice.Infrastrcure.Persistent.Repositories;

public class InvoiceRepository : Repo<Model.Entities.Invoice>
{
    public InvoiceRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
