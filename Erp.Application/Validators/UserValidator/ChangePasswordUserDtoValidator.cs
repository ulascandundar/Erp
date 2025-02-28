using Erp.Domain.DTOs.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.UserValidator;

public class ChangePasswordUserDtoValidator : AbstractValidator<ChangePasswordUserDto>
{
	public ChangePasswordUserDtoValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Parola boş olamaz")
			.NotNull().WithMessage("Parola boş olamaz")
			.MinimumLength(6).WithMessage("Parola en az 6 karakterden oluşmalıdır");
	}
}
