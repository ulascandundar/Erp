using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto);
    Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto, Guid productId);
    Task HardDeleteProductAsync(Guid id);
    Task SoftDeleteProductAsync(Guid id);
    Task<CustomPagedResult<ProductDto>> GetPagedAsync(PaginationRequest paginationRequest);
} 