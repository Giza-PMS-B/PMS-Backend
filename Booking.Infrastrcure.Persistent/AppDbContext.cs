using Booking.Model.Entities;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastrcure.Persistent;

public class AppDbContext : DbContext
{
    public DbSet<Site> Sites { get; set; }
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

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("");
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}