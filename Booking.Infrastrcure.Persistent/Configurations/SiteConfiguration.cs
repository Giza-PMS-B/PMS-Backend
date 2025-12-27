using System;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Booking.Infrastrcure.Persistent.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
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
        builder.Property(site => site.Path).IsRequired();
        builder.Property(site => site.PricePerHour)
        .HasColumnType("decimal(18,2)");
        builder.Property(site => site.IsLeaf).IsRequired();

        builder
            .HasMany(s => s.Tickets)
            .WithOne(t => t.Site)
            .HasForeignKey(t => t.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
