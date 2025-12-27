using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Site.Model.Entities;

namespace Site.Infrastrcure.Persistent.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Model.Entities.Site>
{
    public void Configure(EntityTypeBuilder<Model.Entities.Site> builder)
    {
        builder.ToTable("Sites");

        builder.HasKey(site => site.Id);
        builder.Property(site => site.IntegrationCode);
        builder.Property(site => site.NameAr)
        .IsRequired()
        .HasMaxLength(100);

        builder.Property(site => site.NameEn)
        .IsRequired()
        .HasMaxLength(100);

        builder.Property(site => site.NumberOfSolts);
        builder.Property(site => site.PricePerHour)
        .HasColumnType("decimal(18,2)");

        builder.Property(site => site.Path).IsRequired();

        builder.Property(site => site.IsLeaf).IsRequired();

        builder.Property(site => site.ParentId);

        builder
             .HasOne(s => s.Parent)
             .WithMany(s => s.Children)
             .HasForeignKey(s => s.ParentId)
             .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(s => s.Polygons)
            .WithOne(p => p.Site)
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
