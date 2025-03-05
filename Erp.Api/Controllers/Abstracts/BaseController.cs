using ClosedXML.Excel;
using Erp.Domain.Models;
using Erp.Infrastructure.Attributes.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection;
using System.Text;

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

	public static FileContentResult ExportToExcel<T>(List<T> data, string fileName)
	{
		using (var workbook = new XLWorkbook())
		{
			var worksheet = workbook.Worksheets.Add("Sheet1");

			// Verileri bir DataTable'e dönüştür
			var dataTable = new DataTable();
			var properties = typeof(T).GetProperties();

			foreach (var property in properties)
			{
				// Attribute'dan kolon adını al, yoksa property adını kullan
				var columnNameAttribute = property.GetCustomAttribute<ExcelColumnNameAttribute>();
				string columnName = columnNameAttribute != null ? columnNameAttribute.ColumnName : property.Name;

				dataTable.Columns.Add(columnName, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
			}

			foreach (var item in data)
			{
				var row = dataTable.NewRow();
				foreach (var property in properties)
				{
					// Attribute'dan kolon adını al, yoksa property adını kullan
					var columnNameAttribute = property.GetCustomAttribute<ExcelColumnNameAttribute>();
					string columnName = columnNameAttribute != null ? columnNameAttribute.ColumnName : property.Name;

					row[columnName] = property.GetValue(item);
				}
				dataTable.Rows.Add(row);
			}

			// DataTable'i Excel sayfasına ekleyin
			worksheet.Cell(1, 1).InsertTable(dataTable);

			// Excel dosyasını oluşturmak için MemoryStream kullanın
			using (var stream = new MemoryStream())
			{
				workbook.SaveAs(stream);
				stream.Position = 0;

				var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				return new FileContentResult(stream.ToArray(), contentType)
				{
					FileDownloadName = fileName + ".xlsx"
				};
			}
		}
	}

	public static FileContentResult ExportToCsv<T>(List<T> data, string fileName)
	{
		var properties = typeof(T).GetProperties();
		var columnNames = new List<string>();

		// Get column names from attributes or property names
		foreach (var property in properties)
		{
			var columnNameAttribute = property.GetCustomAttribute<ExcelColumnNameAttribute>();
			string columnName = columnNameAttribute != null ? columnNameAttribute.ColumnName : property.Name;
			columnNames.Add(columnName);
		}

		// Create CSV content
		var sb = new StringBuilder();

		// Add header row
		sb.AppendLine(string.Join(",", columnNames.Select(EscapeCsvField)));

		// Add data rows
		foreach (var item in data)
		{
			var values = new List<string>();
			foreach (var property in properties)
			{
				var value = property.GetValue(item);
				values.Add(EscapeCsvField(value?.ToString() ?? string.Empty));
			}
			sb.AppendLine(string.Join(",", values));
		}

		// Convert to byte array
		var bytes = Encoding.UTF8.GetBytes(sb.ToString());

		// Return as file
		var contentType = "text/csv";
		return new FileContentResult(bytes, contentType)
		{
			FileDownloadName = fileName + ".csv"
		};
	}

	private static string EscapeCsvField(string field)
	{
		// If the field contains a comma, newline, or double quote, it needs to be escaped
		if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
		{
			// Double up any double quotes and enclose the entire field in double quotes
			return "\"" + field.Replace("\"", "\"\"") + "\"";
		}
		return field;
	}
}