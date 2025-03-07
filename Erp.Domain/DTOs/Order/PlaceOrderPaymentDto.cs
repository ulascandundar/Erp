using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order;

public class PlaceOrderPaymentDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string PaymentMethod { get; set; }
}