using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.User;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.UserValidator;

public class ChangePasswordUserDtoValidator : AbstractValidator<ChangePasswordUserDto>
{
	public ChangePasswordUserDtoValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.Password)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordRequired)
			.MinimumLength(6).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordMinLength);
	}
}
