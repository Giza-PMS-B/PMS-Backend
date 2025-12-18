using System;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Booking.Infrastrcure.Persistent.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.SiteName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ticket => ticket.PlateNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ticket => ticket.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(ticket => ticket.BookingFrom)
            .IsRequired();

        builder.Property(ticket => ticket.BookingTo)
            .IsRequired();

        builder.Property(ticket => ticket.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder
            .HasOne(t => t.Site)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
