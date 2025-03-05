using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.PenginUser;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.PendingUserValidator;

public class PendingUserCreateDtoValidator : AbstractValidator<PendingUserCreateDto>
{
	public PendingUserCreateDtoValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.Tckn)
			.NotEmpty().WithRequiredMessage(localizationService, "TCKN")
			.Length(11).WithLocalizedMessage(localizationService, ResourceKeys.Validation.TcknLength);
		
		RuleFor(x => x.FirstName)
			.NotEmpty().WithRequiredMessage(localizationService, "First Name")
			.MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.FirstNameMaxLength);
		
		RuleFor(x => x.LastName)
			.NotEmpty().WithRequiredMessage(localizationService, "Last Name")
			.MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.LastNameMaxLength);
		
		RuleFor(x => x.Email)
			.NotEmpty().WithRequiredMessage(localizationService, "Email")
			.EmailAddress().WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidEmail);
		
		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithRequiredMessage(localizationService, "Phone Number")
			.Length(10).WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidPhoneNumber);
		
		RuleFor(x => x.Address)
			.NotEmpty().WithRequiredMessage(localizationService, "Address")
			.MaximumLength(250).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CompanyAddressMaxLength);
		
		RuleFor(x => x.City)
			.NotEmpty().WithRequiredMessage(localizationService, "City")
			.MaximumLength(50).WithLocalizedMessage(localizationService, ResourceKeys.Validation.CityMaxLength);
		
		RuleFor(x => x.Password)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordRequired)
			.MinimumLength(6).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordMinLength);
	}
}
