using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Category;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;

namespace Erp.Application.Validators.CategoryValidator;

public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.CategoryNameRequired)
            .MaximumLength(100).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CategoryNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CategoryDescriptionMaxLength);
    }
} 