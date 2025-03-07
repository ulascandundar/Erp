using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
}