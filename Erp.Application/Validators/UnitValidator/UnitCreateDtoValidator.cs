using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Unit;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;

namespace Erp.Application.Validators.UnitValidator;

public class UnitCreateDtoValidator : AbstractValidator<UnitCreateDto>
{
    public UnitCreateDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitNameRequired)
            .MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitNameMaxLength);

        RuleFor(x => x.ShortCode)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitShortCodeRequired)
            .MaximumLength(10).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitShortCodeMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitDescriptionMaxLength);

        RuleFor(x => x.ConversionRate)
            .GreaterThan(0).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitConversionRateGreaterThanZero);

        RuleFor(x => x.RootUnitId)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UnitRootUnitIdRequired);
    }
} 