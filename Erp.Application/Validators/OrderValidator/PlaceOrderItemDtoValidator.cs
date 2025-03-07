using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;

namespace Erp.Application.Validators.OrderValidator;

public class PlaceOrderItemDtoValidator : AbstractValidator<PlaceOrderItemDto>
{
    public PlaceOrderItemDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.ProductRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.QuantityGreaterThanZero);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.TotalAmountNotNegative);
    }
}