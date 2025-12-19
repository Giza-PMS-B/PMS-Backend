using Booking.Application.DTO;
using FluentValidation;
using System;

namespace Booking.Application.FluentValidation;

public class CreateSiteDTOValidator:AbstractValidator<CreateSiteDTO>
{
    public CreateSiteDTOValidator()
    {
        RuleFor(x => x.Path)
                .NotEmpty().WithMessage("Path is required")
                .MaximumLength(500).WithMessage("Path cannot exceed 500 characters")
                .Must(path => !path.Contains("..")).WithMessage("Path cannot contain '..'")
                .Must(path => path.StartsWith("/")).WithMessage("Path must start with '/'");


        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .Length(3, 100).WithMessage("English name must be between 3 and 100 characters")
            .Matches(@"^[A-Za-z\s\-_,.]+$").WithMessage("Please enter valid input using English letters, and the following special characters: [ - , _ , space, .]")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("English name cannot be only whitespace");

        
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .Length(3, 100).WithMessage("Arabic name must be between 3 and 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Arabic name cannot be only whitespace");

       
        RuleFor(x => x.PricePerHour)
            .GreaterThan(0).WithMessage("Price per hour must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Price per hour cannot exceed 10,000");

        
        RuleFor(x => x.IntegrationCode)
            .NotEmpty().WithMessage("Integration code is required")
            .MaximumLength(100).WithMessage("Integration code cannot exceed 50 characters")
            .MinimumLength(1).WithMessage("Integration code is required")
            .Matches(@"^[A-Za-z0-9\s\-_,.]+$").WithMessage("Integration code can only contain letters, numbers, spaces, hyphens, underscores, dots, and commas")
            .Must(code => !string.IsNullOrWhiteSpace(code)).WithMessage("Integration code cannot be only whitespace");

        
        RuleFor(x => x.NumberOfSlots)
            .GreaterThanOrEqualTo(0).WithMessage("Number of slots cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Number of slots cannot exceed 1000");

        
        RuleFor(x => x.IsLeaf)
            .NotNull().WithMessage("IsLeaf flag is required");
    }

}
