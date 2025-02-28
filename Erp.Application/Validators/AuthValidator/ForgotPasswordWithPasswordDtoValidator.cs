using Erp.Domain.DTOs.Auth;
using FluentValidation;

namespace Erp.Application.Validators.AuthValidator;

public class ForgotPasswordWithPasswordDtoValidator : AbstractValidator<ForgotPasswordWithPasswordDto>
{
	public ForgotPasswordWithPasswordDtoValidator()
	{
		RuleFor(x => x.Email).NotEmpty().WithMessage("Email boş olamaz")
			.NotNull().WithMessage("Email boş olamaz")
			.EmailAddress().WithMessage("Geçersiz email adresi");

		RuleFor(x => x.Otp).NotEmpty().WithMessage("Otp boş olamaz");

		RuleFor(x => x.Password).NotEmpty().WithMessage("Parola boş olamaz")
			.NotNull().WithMessage("Parola boş olamaz")
			.MinimumLength(6).WithMessage("Parola en az 6 karakterden oluşmalıdır");
	}
}
