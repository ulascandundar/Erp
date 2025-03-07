using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal NetAmount { get; set; } // TotalAmount - DiscountAmount
    public decimal DiscountAmount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Description { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();
    public List<Discount> Discounts { get; set; } = new List<Discount>();

    public void AssignToCompanyAndUser(Guid companyId, Guid userId)
    {
        CompanyId = companyId;
        UserId = userId;
    }
}
