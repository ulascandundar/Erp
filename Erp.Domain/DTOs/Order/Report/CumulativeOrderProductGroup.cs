using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order.Report;

public class CumulativeOrderProductGroup
{
	public Guid ProductId { get; set; }
	public string ProductName { get; set; }
	public decimal TotalAmount { get; set; }
	public decimal TotalQuantityCount { get; set; }
}
