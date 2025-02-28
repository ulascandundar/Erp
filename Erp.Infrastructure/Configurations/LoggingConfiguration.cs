using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

public static class LoggingConfiguration
{
	public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
	{
		// Get environment information
		var environment = builder.Environment;
		var configuration = builder.Configuration;

		// Configure Serilog
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration) // Read settings from appsettings.json
			.Enrich.FromLogContext()
			.Enrich.WithExceptionDetails() // Add detailed exception information
			.Enrich.WithMachineName() // Add server name
			.Enrich.WithProperty("Environment", environment.EnvironmentName)
			.Enrich.WithProperty("Application", environment.ApplicationName)
			// Write to console with different format based on environment
			.WriteTo.Console(environment.IsDevelopment()
				? LogEventLevel.Information
				: LogEventLevel.Warning)
			// JSON structured logs for files
			.WriteTo.File(new CompactJsonFormatter(),
				 Path.Combine("logs", "log-.json"),
				 rollingInterval: RollingInterval.Day,
				 retainedFileCountLimit: 30)
			// Plain text logs for easier reading
			.WriteTo.File(
				 Path.Combine("logs", "log-.txt"),
				 rollingInterval: RollingInterval.Day,
				 retainedFileCountLimit: 30)
			// SQL Server logging (uncomment and configure if needed)
			// .WriteTo.MSSqlServer(
			//     connectionString: configuration.GetConnectionString("LoggingDb"),
			//     tableName: "Logs", 
			//     autoCreateSqlTable: true)
			.CreateLogger();

		// Configure built-in logging to use Serilog
		builder.Host.UseSerilog();

		return builder;
	}
}
