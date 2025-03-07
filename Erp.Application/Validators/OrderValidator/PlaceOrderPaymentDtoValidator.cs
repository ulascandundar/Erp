using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;

namespace Erp.Application.Validators.OrderValidator;

public class PlaceOrderPaymentDtoValidator : AbstractValidator<PlaceOrderPaymentDto>
{
    public PlaceOrderPaymentDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.PaymentAmountGreaterThanZero);

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.PaymentMethodRequired)
            .Must(BeValidPaymentMethod)
            .WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidPaymentMethod);
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        return paymentMethod == PaymentMethods.Cash ||
               paymentMethod == PaymentMethods.CreditCard ||
               paymentMethod == PaymentMethods.Wallet ||
               paymentMethod == PaymentMethods.MealCard ||
               paymentMethod == PaymentMethods.Discount ||
               paymentMethod == PaymentMethods.Other;
    }
}