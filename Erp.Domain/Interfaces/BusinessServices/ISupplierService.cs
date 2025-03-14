using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Supplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface ISupplierService
{
    Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto supplierCreateDto);
    Task<SupplierDto> UpdateSupplierAsync(Guid id, SupplierUpdateDto supplierUpdateDto);
    Task DeleteSupplierAsync(Guid id);
    Task<SupplierDto> GetSupplierByIdAsync(Guid id);
    Task<CustomPagedResult<SupplierDto>> GetSuppliersAsync(PaginationRequest paginationRequest);
} 