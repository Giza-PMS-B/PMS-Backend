using FluentValidation;
using Invoice.Application.DTO;
using System;

namespace Invoice.Application.FluentValidation;

public class CreateInvoiceDTOValidator : AbstractValidator<CreateInvoiceDTO>
{
    public CreateInvoiceDTOValidator()
    {
        RuleFor(x => x.HtmlDocumentPath)
               .NotEmpty().WithMessage("HTML document path is required")
               .MaximumLength(500).WithMessage("HTML document path cannot exceed 500 characters");

        RuleFor(x => x.TicketSerialNumber)
            .NotEmpty().WithMessage("Ticket serial number is required")
            .Length(9).WithMessage("Ticket serial number must be 9 characters");

        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required")
            .NotEqual(Guid.Empty).WithMessage("Invalid Ticket ID");
    }
}
