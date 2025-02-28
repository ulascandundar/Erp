using Erp.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Abstracts;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseController : ControllerBase
{
	protected IActionResult CustomResponse(object data = null, string message = "Operation successful", bool isSuccess = true)
	{
		var response = new CustomResponseModel
		{
			IsSuccess = isSuccess,
			Message = message,
			Data = data
		};

		return Ok(response);
	}

	protected IActionResult CustomError(string message = "Operation failed", object data = null)
	{
		var response = new CustomResponseModel
		{
			IsSuccess = false,
			Message = message,
			Data = data
		};

		return BadRequest(response);
	}
}