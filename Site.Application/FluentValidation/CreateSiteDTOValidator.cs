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

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.");
    }
}
