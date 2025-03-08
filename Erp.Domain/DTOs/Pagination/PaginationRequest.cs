using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Pagination;

public class PaginationRequest
{
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public bool IsDesc { get; set; }
	public string OrderBy { get; set; }
	public string Search { get; set; }
	public string Query { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}
