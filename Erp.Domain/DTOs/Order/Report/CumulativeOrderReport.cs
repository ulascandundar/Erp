using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order.Report;

public class CumulativeOrderReport
{
	public decimal TotalAmount { get; set; }
	public decimal TotalDiscount { get; set; }
	public decimal TotalNetAmount { get; set; }
	public decimal TotalOrderCount { get; set; }
	public List<CumulativeOrderPaymentMethodGroup> PaymentMethodGroups { get; set; }
}
