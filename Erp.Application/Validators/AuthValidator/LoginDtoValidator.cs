using Erp.Domain.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.AuthValidator;

public class LoginDtoValidator : AbstractValidator<UserLoginDto>
{
	public LoginDtoValidator()
	{
		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
			.When(x => !string.IsNullOrEmpty(x.Email));
	}
}
