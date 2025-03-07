using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order.Report;

public class CumulativeOrderProductGroupDto
{
	public decimal TotalAmount { get; set; }
	public decimal TotalQuantityCount { get; set; }
	public List<CumulativeOrderProductGroup> ProductGroups { get; set; }
}
