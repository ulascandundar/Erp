using System;

namespace Erp.Domain.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Barcode { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 