using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Linq;

namespace Erp.Application.Validators.OrderValidator;

public class PlaceOrderDtoValidator : AbstractValidator<PlaceOrderDto>
{
    public PlaceOrderDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.OrderItemsRequired);

        RuleFor(x => x.Payments)
            .NotEmpty()
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.PaymentsRequired);

        RuleForEach(x => x.OrderItems)
            .SetValidator(new PlaceOrderItemDtoValidator(localizationService));

        RuleForEach(x => x.Payments)
            .SetValidator(new PlaceOrderPaymentDtoValidator(localizationService));
    }
} 