using System;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoice.Infrastrcure.Persistent.Configurations;

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

        // 1:1 relationship with Invoice
        builder.HasOne(t => t.Invoice)
               .WithOne(i => i.Ticket)
               .HasForeignKey<Model.Entities.Invoice>(i => i.TicketId);

    }
}
