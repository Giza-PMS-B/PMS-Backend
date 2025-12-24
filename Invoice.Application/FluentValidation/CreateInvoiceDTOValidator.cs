using FluentValidation;
using Invoice.Application.DTO;
using System;

namespace Invoice.Application.FluentValidation;

public class CreateInvoiceDTOValidator : AbstractValidator<CreateInvoiceDTO>
{
    public CreateInvoiceDTOValidator()
    {
       
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required")
            .NotEqual(Guid.Empty).WithMessage("Invalid Ticket ID");
    }
}
