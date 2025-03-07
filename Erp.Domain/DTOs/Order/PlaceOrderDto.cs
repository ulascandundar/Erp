using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order;

public class PlaceOrderDto
{
    public List<PlaceOrderItemDto> OrderItems { get; set; }
    public List<PlaceOrderPaymentDto> Payments { get; set; }
    public string Description { get; set; }
    public bool IsSafeOrder { get; set; } = true;
}