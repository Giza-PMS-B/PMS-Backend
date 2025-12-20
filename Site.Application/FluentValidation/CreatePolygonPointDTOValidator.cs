using FluentValidation;
using Site.Application.DTO;
using System;

namespace Site.Application.FluentValidation;

public class CreatePolygonPointDTOValidator : AbstractValidator<CreatePolygonPointDTO>
{
    public CreatePolygonPointDTOValidator()
    {
        RuleFor(x => x.Latitude)
            .NotEmpty().WithMessage("Latitude is required")
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and +90")
            .PrecisionScale(8, 6, false).WithMessage("Latitude can have up to 6 decimal places");

        RuleFor(x => x.Longitude)
            .NotEmpty().WithMessage("Longitude is required")
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and +180")
            .PrecisionScale(9, 6, false).WithMessage("Longitude can have up to 6 decimal places");
    }
}
