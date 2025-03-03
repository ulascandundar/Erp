using System;

namespace Erp.Domain.DTOs.Product;

public class ProductCreateDto
{
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Barcode { get; set; }
} 