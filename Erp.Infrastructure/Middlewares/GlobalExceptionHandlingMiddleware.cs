using Erp.Domain.CustomExceptions;
using Erp.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

	public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		// Determine HTTP status code based on exception type
		HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default 500

		if (exception is BaseException baseException)
		{
			statusCode = baseException.StatusCode;
		}

		// Log exception if it's not NonLoggableException
		if (!(exception is NonLoggableException))
		{
			if (exception is LoggableException || exception is Exception)
			{
				_logger.LogError(exception, "An error occurred: {Message}", exception.Message);
			}
		}

		// Create custom response
		var response = new CustomResponseModel
		{
			IsSuccess = false,
			Message = exception.Message,
			Data = null
		};

		// Set response details
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		// Serialize and write the response
		var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		await context.Response.WriteAsync(jsonResponse);
	}
}
