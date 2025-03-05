using Erp.Application.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Validators.PaginationValidator;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
	public PaginationRequestValidator(ILocalizationService localizationService)
	{
		RuleFor(x => x.PageNumber)
			.GreaterThan(0).WithLocalizedMessage(localizationService, ResourceKeys.Validation.PageNumberGreaterThanZero);
		
		RuleFor(x => x.PageSize)
			.GreaterThan(0).WithLocalizedMessage(localizationService, ResourceKeys.Validation.PageSizeGreaterThanZero);
	}
}
