using Erp.Domain.DTOs.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
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
}
