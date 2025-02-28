using Erp.Domain.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.AuthValidator;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
	public ForgotPasswordDtoValidator()
	{
		RuleFor(x => x.Email).NotEmpty().WithMessage("Email boş olamaz");
		RuleFor(x => x.Email).NotNull().WithMessage("Email boş olamaz");
		RuleFor(x => x.Email).EmailAddress().WithMessage("Geçersiz email adresi");
	}
}
