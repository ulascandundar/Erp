using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Description { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public List<OrderPaymentDto> OrderPayments { get; set; }
    public DateTime CreatedAt { get; set; }
} 