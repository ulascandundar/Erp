using Erp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Erp.Api.Filters;

public class CustomValidationFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.ModelState.IsValid)
		{
			var errors = context.ModelState
				.Where(e => e.Value.Errors.Count > 0)
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
				);

			var response = new CustomResponseModel
			{
				IsSuccess = false,
				Message = "Validation failed",
				Data = errors
			};

			context.Result = new BadRequestObjectResult(response);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context) { }
}
