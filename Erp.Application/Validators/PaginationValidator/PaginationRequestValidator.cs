using Erp.Domain.DTOs.Pagination;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.PaginationValidator;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
	public PaginationRequestValidator()
	{
		RuleFor(x => x.PageNumber).GreaterThan(0);
		RuleFor(x => x.PageSize).GreaterThan(0);
	}
}
