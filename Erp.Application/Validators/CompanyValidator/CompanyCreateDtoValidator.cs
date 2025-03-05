using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Company;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;

namespace Erp.Application.Validators.CompanyValidator;

public class CompanyCreateDtoValidator : AbstractValidator<CompanyCreateDto>
{
    public CompanyCreateDtoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.CompanyNameRequired)
            .MaximumLength(100).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CompanyNameMaxLength);

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.CompanyTaxNumberRequired)
            .Length(10).WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidTaxNumber)
            .Matches("^[0-9]*$").WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidTaxNumber);

        RuleFor(x => x.Email)
            .EmailAddress().WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidEmail)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithRequiredMessage(localizationService, "Phone Number")
            .Length(10).WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidPhoneNumber)
            .Matches("^[0-9]*$").WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidPhoneNumber);

        RuleFor(x => x.Address)
            .NotEmpty().WithRequiredMessage(localizationService, "Address")
            .MaximumLength(250).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CompanyAddressMaxLength);

        RuleFor(x => x.Website)
            .MaximumLength(100).WithLocalizedMessage(localizationService, ResourceKeys.Validation.WebsiteMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithLocalizedMessage(localizationService, ResourceKeys.Validation.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}