using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Pagination;

public class CustomPagedResult<T> where T : class
{
	public List<T> Items { get; set; }
	public int TotalPages { get; set; }
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
}
