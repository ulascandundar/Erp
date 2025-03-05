using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.AuthValidator;

public class LoginDtoValidator : AbstractValidator<UserLoginDto>
{
	public LoginDtoValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.Email)
			.EmailAddress().WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidEmail)
			.When(x => !string.IsNullOrEmpty(x.Email));
	}
}
