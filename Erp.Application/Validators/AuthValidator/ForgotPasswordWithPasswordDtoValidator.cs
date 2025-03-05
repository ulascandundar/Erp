using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;

namespace Erp.Application.Validators.AuthValidator;

public class ForgotPasswordWithPasswordDtoValidator : AbstractValidator<ForgotPasswordWithPasswordDto>
{
	public ForgotPasswordWithPasswordDtoValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithRequiredMessage(localizationService, "Email")
			.EmailAddress().WithLocalizedMessage(localizationService, ResourceKeys.Validation.InvalidEmail);

		RuleFor(x => x.Otp)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.OtpRequired);

		RuleFor(x => x.Password)
			.NotEmpty().WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordRequired)
			.MinimumLength(6).WithLocalizedMessage(localizationService, ResourceKeys.Validation.UserPasswordMinLength);
	}
}
