using FluentValidation;
using Site.Application.DTO;
using System;

namespace Site.Application.FluentValidation;

public class CreateSiteDTOValidator : AbstractValidator<CreateSiteDTO>
{
    public CreateSiteDTOValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("Path is required")
            .MaximumLength(500).WithMessage("Path cannot exceed 500 characters")
            .Must(path => !path.Contains("..")).WithMessage("Path cannot contain '..'")
            .Must(path => path.StartsWith("/")).WithMessage("Path must start with '/'");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Name (EN) is required")
            .Length(3, 100).WithMessage("Name (EN) must be between 3 and 100 characters");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name (AR) is required")
            .Length(3, 100).WithMessage("Name (AR) must be between 3 and 100 characters");

        RuleFor(x => x.PricePerHour)
            .NotEmpty().WithMessage("Price per hour is required")
            .GreaterThan(0).WithMessage("Price per hour must be greater than 0")
            .PrecisionScale(18, 2, false).WithMessage("Price per hour can have up to 2 decimal places");

        RuleFor(x => x.IntegrationCode)
            .NotEmpty().WithMessage("Integration Code is required")
            .Length(3, 100).WithMessage("Integration Code must be between 3 and 100 characters")
            .Matches(@"^[a-zA-Z0-9\-_ .]+$").WithMessage("Integration Code can only contain letters, numbers, '-', '_', space, and '.'");

        RuleFor(x => x.NumberOfSolts)
            .NotEmpty().WithMessage("Number of slots is required")
            .InclusiveBetween(1, 10000).WithMessage("Number of slots must be between 1 and 10,000");

        RuleFor(x => x.Polygons)
            .NotEmpty().WithMessage("At least one polygon is required")
            .Must(polygons => polygons != null && polygons.Count > 0)
            .WithMessage("At least one polygon must be added to define boundaries");

        RuleForEach(x => x.Polygons)
            .SetValidator(new CreatePolygonDTOValidator());
    }
}
