using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Site.Model.Entities;
namespace Site.Infrastrcure.Persistent;

public class AppDbContext : DbContext
{
    public DbSet<Model.Entities.Site> Sites { get; set; }
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
            optionsBuilder.UseSqlServer("Server=pms-sqlserver,1433; Database=PMS_Site; User Id=sa; Password=YourStrong@Passw0rd; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}