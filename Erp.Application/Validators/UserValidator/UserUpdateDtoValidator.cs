using Erp.Domain.DTOs.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.UserValidator;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
	public UserUpdateDtoValidator()
	{
		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
			.When(x => !string.IsNullOrEmpty(x.Email));
		RuleFor(x => x.Roles)
			.NotEmpty().WithMessage("Rol boş olamaz")
			.Must(roles => roles != null && roles.Any())
			.WithMessage("En az 1 adet rol seçilmelidir");
	}
}
