using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoice.Infrastrcure.Persistent.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Model.Entities.Invoice>
{
    public void Configure(EntityTypeBuilder<Model.Entities.Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.HtmlDocument)
            .IsRequired();

        builder.Property(invoice => invoice.TicketSerialNumber)
          .IsRequired();

        builder.Property(invoice => invoice.TaxAmount)
             .IsRequired()
             .HasColumnType("decimal(18,2)");

        builder.Property(invoice => invoice.TotalAmountAfterTax)
             .IsRequired()
             .HasColumnType("decimal(18,2)");

        builder.Property(invoice => invoice.TotalAmountBeforeTax)
             .IsRequired()
             .HasColumnType("decimal(18,2)");
    }
}
