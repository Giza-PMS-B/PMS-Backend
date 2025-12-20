using FluentValidation;
using Site.Application.DTO;
using System;

namespace Site.Application.FluentValidation;

public class CreatePolygonDTOValidator : AbstractValidator<CreatePolygonDTO>
{
    public CreatePolygonDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Polygon name is required")
            .Length(3, 100).WithMessage("Polygon name must be between 3 and 100 characters");

        RuleFor(x => x.Points)
            .NotEmpty().WithMessage("Polygon must have at least 3 coordinate points")
            .Must(points => points != null && points.Count >= 3)
            .WithMessage("Polygon must have at least 3 coordinate points");

        RuleForEach(x => x.Points)
            .SetValidator(new CreatePolygonPointDTOValidator());
    }
}
