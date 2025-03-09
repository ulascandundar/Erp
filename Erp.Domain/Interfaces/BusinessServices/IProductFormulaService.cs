using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.ProductFormula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IProductFormulaService
{
    Task<ProductFormulaDto> CreateProductFormulaAsync(ProductFormulaCreateDto productFormulaCreateDto);
    Task<ProductFormulaDto> UpdateProductFormulaAsync(Guid id, ProductFormulaUpdateDto productFormulaUpdateDto);
    Task DeleteProductFormulaAsync(Guid id);
    Task<ProductFormulaDto> GetProductFormulaAsync(Guid id);
    Task<CustomPagedResult<ProductFormulaDto>> GetProductFormulasAsync(PaginationRequest paginationRequest);
} 