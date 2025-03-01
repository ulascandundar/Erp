using Erp.Domain.DTOs.Company;
using Erp.Domain.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllCompaniesAsync();
    Task<CompanyDto> GetCompanyByIdAsync(Guid id);
    Task<CompanyDto> CreateCompanyAsync(CompanyCreateDto companyCreateDto);
    Task<CompanyDto> UpdateCompanyAsync(CompanyUpdateDto companyUpdateDto, Guid companyId);
    Task HardDeleteCompanyAsync(Guid id);
    Task SoftDeleteCompanyAsync(Guid id);
    Task<CustomPagedResult<CompanyDto>> GetPagedAsync(PaginationRequest paginationRequest);
} 