using FluentValidation;
using Site.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.FluentValidation
{
    internal class CreateLeafSiteDTOValidator : AbstractValidator<CreateLeafSiteDTO>
    {
        public CreateLeafSiteDTOValidator()
        {
            Include(new CreateSiteDTOValidator());

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
                .WithMessage("At least one polygon must be added to define boundaries")
                .Must(polygons => !HasDuplicatePolygonNames(polygons))
                .WithMessage("These Values are already exists");

            RuleForEach(x => x.Polygons)
                .SetValidator(new CreatePolygonDTOValidator());
        }

        private bool HasDuplicatePolygonNames(List<CreatePolygonDTO> polygons)
        {
            if (polygons == null || polygons.Count == 0) return false;

            var distinctNames = polygons
                .Select(p => p.Name?.Trim().ToLower())
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .Count();

            return distinctNames < polygons.Count;
        }

    }
}

