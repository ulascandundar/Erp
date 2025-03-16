using Erp.Domain.DTOs.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp.Application.Common.Extensions;

public static class QueryableExtensions
{
	public static async Task<CustomPagedResult<T>> ToPagedResultAsync<T>(
		this IQueryable<T> query,
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken = default) where T : class
	{
		var totalCount = await query.CountAsync(cancellationToken);
		if (!string.IsNullOrEmpty(paginationRequest.OrderBy))
		{
			query = query.OrderBy(paginationRequest.OrderBy + (paginationRequest.IsDesc ? " desc" : " asc"));
		}
		var items = await query
			.Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.ToListAsync(cancellationToken);
		var result = new CustomPagedResult<T>
		{
			Items = items,
			TotalCount = totalCount,
			TotalPages = (int)Math.Ceiling(totalCount / (double)paginationRequest.PageSize),
			PageNumber = paginationRequest.PageNumber,
			PageSize = paginationRequest.PageSize
		};
		return result;
	}
	public static IQueryable<T> ApplyQueryBuilderFilter<T>(this IQueryable<T> query, string filterJson)
	{
		if (string.IsNullOrEmpty(filterJson))
			return query;

		// URL decode the filterJson if it's encoded
		filterJson = WebUtility.UrlDecode(filterJson);

		try
		{
			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			var filter = JsonSerializer.Deserialize<QueryBuilderFilter>(filterJson, options);
			if (filter == null || !filter.Valid || filter.Rules == null || !filter.Rules.Any())
				return query;

			var parameter = Expression.Parameter(typeof(T), "x");
			var expression = BuildExpression<T>(filter, parameter);

			if (expression != null)
			{
				var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
				return query.Where(lambda);
			}
		}
		catch (Exception ex)
		{
			// Log the exception
			Console.WriteLine($"Error parsing filter: {ex.Message}");
		}

		return query;
	}

	private static Expression BuildExpression<T>(QueryBuilderFilter filter, ParameterExpression parameter)
	{
		if (filter.Rules == null || !filter.Rules.Any())
			return null;

		var expressions = new List<Expression>();

		foreach (var rule in filter.Rules)
		{
			if (rule.Rules != null && rule.Rules.Any()) // This is a group
			{
				var groupExpression = BuildExpression<T>(rule, parameter);
				if (groupExpression != null)
					expressions.Add(groupExpression);
			}
			else if (!string.IsNullOrEmpty(rule.Field)) // This is a rule
			{
				var propertyExpression = Expression.Property(parameter, rule.Field);
				var valueExpression = GetValueExpression(rule.Value, propertyExpression.Type);

				Expression comparison = null;
				switch (rule.Operator.ToLower())
				{
					case "equal":
						comparison = Expression.Equal(propertyExpression, valueExpression);
						break;
					case "not_equal":
						comparison = Expression.NotEqual(propertyExpression, valueExpression);
						break;
					case "greater":
						comparison = Expression.GreaterThan(propertyExpression, valueExpression);
						break;
					case "greater_or_equal":
						comparison = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
						break;
					case "less":
						comparison = Expression.LessThan(propertyExpression, valueExpression);
						break;
					case "less_or_equal":
						comparison = Expression.LessThanOrEqual(propertyExpression, valueExpression);
						break;
					case "contains":
						var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
						comparison = Expression.Call(propertyExpression, containsMethod, valueExpression);
						break;
					case "begins_with":
						var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
						comparison = Expression.Call(propertyExpression, startsWithMethod, valueExpression);
						break;
					case "ends_with":
						var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
						comparison = Expression.Call(propertyExpression, endsWithMethod, valueExpression);
						break;
					case "is_null":
						comparison = Expression.Equal(propertyExpression, Expression.Constant(null, propertyExpression.Type));
						break;
					case "is_not_null":
						comparison = Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type));
						break;
					case "in":
						// Assuming the value is a list of values
						if (rule.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
						{
							var listType = typeof(List<>).MakeGenericType(propertyExpression.Type);
							var list = Activator.CreateInstance(listType);
							var addMethod = listType.GetMethod("Add");

							foreach (var item in jsonElement.EnumerateArray())
							{
								var value = ConvertJsonElement(item, propertyExpression.Type);
								addMethod.Invoke(list, new[] { value });
							}

							var containsMethod2 = listType.GetMethod("Contains", new[] { propertyExpression.Type });
							comparison = Expression.Call(Expression.Constant(list), containsMethod2, propertyExpression);
						}
						break;
				}

				if (comparison != null)
					expressions.Add(comparison);
			}
		}

		if (!expressions.Any())
			return null;

		Expression resultExpression = expressions.First();
		for (int i = 1; i < expressions.Count; i++)
		{
			resultExpression = filter.Condition.ToUpper() == "AND"
				? Expression.AndAlso(resultExpression, expressions[i])
				: Expression.OrElse(resultExpression, expressions[i]);
		}

		return resultExpression;
	}

	private static Expression GetValueExpression(object value, Type targetType)
	{
		if (value is JsonElement jsonElement)
		{
			value = ConvertJsonElement(jsonElement, targetType);
		}

		return Expression.Constant(value, targetType);
	}

	private static object ConvertJsonElement(JsonElement element, Type targetType)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.String:
				return element.GetString();
			case JsonValueKind.Number:
				if (targetType == typeof(int) || targetType == typeof(int?))
					return element.GetInt32();
				else if (targetType == typeof(long) || targetType == typeof(long?))
					return element.GetInt64();
				else if (targetType == typeof(float) || targetType == typeof(float?))
					return element.GetSingle();
				else if (targetType == typeof(double) || targetType == typeof(double?))
					return element.GetDouble();
				else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
					return element.GetDecimal();
				return element.GetDouble();
			case JsonValueKind.True:
			case JsonValueKind.False:
				return element.GetBoolean();
			case JsonValueKind.Null:
				return null;
			default:
				return null;
		}
	}
}


public class QueryBuilderFilter
{
	public string Condition { get; set; } = "AND";
	public List<QueryBuilderRule> Rules { get; set; } = new List<QueryBuilderRule>();
	public bool Valid { get; set; } = true;
}

public class QueryBuilderRule : QueryBuilderFilter
{
	public string Id { get; set; }
	public string Field { get; set; }
	public string Type { get; set; }
	public string Input { get; set; }
	public string Operator { get; set; }
	public object Value { get; set; }
}
