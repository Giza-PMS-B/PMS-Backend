using Microsoft.EntityFrameworkCore;
using Site.Model.Entities;
namespace Site.Infrastrcure.Persistent;

public class AppDbContext : DbContext
{
    public DbSet<site> Sites { get; set; }
    public DbSet<Polygon> Polygons { get; set; }
    public DbSet<PolygonPoint> PolygonPoints { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations in the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}