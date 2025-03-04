using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductCategory;

public class ProductCategoryCreateDto
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }
}