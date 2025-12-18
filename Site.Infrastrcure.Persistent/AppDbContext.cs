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
        // Env.Load();

        // var connectionString = Environment.GetEnvironmentVariable("SITE_DB_CONNECTION");

        // if (string.IsNullOrEmpty(connectionString))
        // {
        //     throw new InvalidOperationException("SITE_DB_CONNECTION environment variable is not set.");
        // }

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