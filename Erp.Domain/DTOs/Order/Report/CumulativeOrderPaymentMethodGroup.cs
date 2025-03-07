using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order.Report;

public class CumulativeOrderPaymentMethodGroup
{
    public string PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int TotalOrderCount { get; set; }
}