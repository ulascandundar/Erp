using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;

namespace Erp.Application.Validators.ProductValidator;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductNameRequired)
            .MaximumLength(100).WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductNameMaxLength);

        RuleFor(x => x.SKU)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductSkuRequired)
            .MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductSkuMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductDescriptionMaxLength);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductPriceGreaterThanZero);

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductBarcodeMaxLength);
    }
} 