using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent.Configurations;

public class PolygonConfiguration : IEntityTypeConfiguration<Polygon>
{
    public void Configure(EntityTypeBuilder<Polygon> builder)
    {
        builder.ToTable("Polygons");

        builder.HasKey(polygon => polygon.Id);

        builder.Property(polygon => polygon.Name)
        .IsRequired()
        .HasMaxLength(100);

        builder.HasIndex(p => p.Name)
        .IsUnique();

        builder
            .HasOne(p => p.Site)
            .WithMany(s => s.Polygons)
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Polygon → PolygonPoints (one-to-many)
        builder
            .HasMany(p => p.PolygonPoints)
            .WithOne(pp => pp.Polygon)
            .HasForeignKey(pp => pp.PolygonId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
