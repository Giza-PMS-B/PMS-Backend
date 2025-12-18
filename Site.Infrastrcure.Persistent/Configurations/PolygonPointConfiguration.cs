using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent.Configurations;

public class PolygonPointConfiguration : IEntityTypeConfiguration<PolygonPoint>
{
    public void Configure(EntityTypeBuilder<PolygonPoint> builder)
    {
        builder.ToTable("PolygonPoints");
        builder.HasKey(pp => new { pp.PolygonId, pp.Longitude, pp.Latitude });

        builder.Property(pp => pp.Longitude)
        .IsRequired()
        .HasColumnType("decimal(9,6)");

        builder.Property(pp => pp.Latitude)
        .IsRequired()
        .HasColumnType("decimal(9,6)");

        builder
            .HasOne(pp => pp.Polygon)
            .WithMany(p => p.PolygonPoints)
            .HasForeignKey(pp => pp.PolygonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
