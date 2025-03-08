using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Customer;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.UserValidator;

public class CustomerDtoValidator : AbstractValidator<CustomerDto>
{

	public CustomerDtoValidator(ILocalizationService localizationService)
	{

		RuleFor(x => x.Tckn)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.TcknRequired))
			.Length(11)
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.TcknLength));

		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.FirstNameRequired));

		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.LastNameRequired));

		RuleFor(x => x.PhoneNumber)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.PhoneNumberRequired))
			.Length(10)
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.PhoneNumberLength));

		RuleFor(x => x.Address)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.AddressRequired));

		RuleFor(x => x.City)
			.NotEmpty()
			.WithLocalizedMessage(localizationService, (ResourceKeys.Validation.CityRequired));
	}
}
