using Microsoft.EntityFrameworkCore;
using Site.Model.Entities;
namespace Site.Infrastrcure.Persistent;

public class AppDbContext : DbContext
{
    public DbSet<site> Sites { get; set; }
    public DbSet<Polygon> Polygons { get; set; }
    public DbSet<PolygonPoint> PolygonPoints { get; set; }

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
            optionsBuilder.UseSqlServer("Server=db35553.public.databaseasp.net; Database=db35553; User Id=db35553; Password=wG-2=t6Z7%Ag; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}