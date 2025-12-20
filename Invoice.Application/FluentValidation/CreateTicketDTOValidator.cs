using FluentValidation;
using Invoice.Application.DTO;
using System;
using System.Text.RegularExpressions;

namespace Invoice.Application.FluentValidation;

public class CreateTicketDTOValidator : AbstractValidator<CreateTicketDTO>
{
    public CreateTicketDTOValidator()
    {
        RuleFor(x => x.SiteName)
                    .NotEmpty().WithMessage("Site name is required")
                    .Length(2, 100).WithMessage("Site name must be between 2 and 100 characters");

        
        RuleFor(x => x.PlateNumber)
                    .NotEmpty().WithMessage("Plate number is required")
                    .Matches(@"^\d{1,4}[A-Z]{3}$").WithMessage("Invalid plate number format. Example: 1234ABC")
                    .Must(BeValidPlateNumber).WithMessage("Invalid plate number");

        
        RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required")
                    .Matches(@"^05\d{8}$").WithMessage("Phone number must be 05 followed by 8 digits (e.g., 0512345678)")
                    .Length(10).WithMessage("Phone number must be exactly 10 digits");

        
        RuleFor(x => x.BookingFrom)
                    .NotEmpty().WithMessage("Booking start time is required");

        RuleFor(x => x.BookingTo)
                    .NotEmpty().WithMessage("Booking end time is required")
                    .GreaterThan(x => x.BookingFrom)
                    .WithMessage("BookingTo must be later than BookingFrom");

        
        RuleFor(x => x.TotalPrice)
                    .GreaterThan(0).WithMessage("Total price must be greater than 0")
                    .LessThanOrEqualTo(10000).WithMessage("Total price cannot exceed 10,000");
    }

    private bool BeValidPlateNumber(string plateNumber)
    {
        if (string.IsNullOrEmpty(plateNumber))
            return false;

        plateNumber = plateNumber.ToUpper();

        var match = Regex.Match(plateNumber, @"^(\d{1,4})([A-Z]{3})$");
        if (!match.Success)
            return false;

        var digitsPart = match.Groups[1].Value;
        return int.TryParse(digitsPart, out int digits) && digits > 0;
    }
}