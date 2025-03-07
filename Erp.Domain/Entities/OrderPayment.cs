using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class OrderPayment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string Description { get; set; }
}