using DotNetEnv;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastrcure.Persistent;

public class AppDbContext : DbContext
{
    public DbSet<Model.Entities.Invoice> Invoices { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    public AppDbContext()
    : base()
    {

    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("INVOICE_DB_CONNECTION");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("INVOICE_DB_CONNECTION environment variable is not set.");
        }

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}