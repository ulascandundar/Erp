using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.DTOs.User;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.UserValidator;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
	public UserCreateDtoValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.Email)
			.EmailAddress().WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidEmail)
			.When(x => !string.IsNullOrEmpty(x.Email));
		
		RuleFor(x => x.Password)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordRequired)
			.MinimumLength(6).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordMinLength);
		
		RuleFor(x => x.Roles)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.RolesRequired)
			.Must(roles => roles != null && roles.Any())
			.WithLocalizedMessage(localizationService, ResourceKeys.Validation.AtLeastOneRoleRequired);
	}
}
