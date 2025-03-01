using Erp.Domain.DTOs.PenginUser;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.PendingUserValidator;

public class PendingUserCreateDtoValidator : AbstractValidator<PendingUserCreateDto>
{
	public PendingUserCreateDtoValidator()
	{
		RuleFor(x => x.Tckn).NotEmpty().Length(11);
		RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
		RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.PhoneNumber).NotEmpty().Length(10); //5077777777
		RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
		RuleFor(x => x.City).NotEmpty().MaximumLength(50);
		RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
	}
}
