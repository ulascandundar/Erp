using AutoMapper;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Company;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Entities;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Attributes.Caching;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Erp.Application.Common.Extensions;

namespace Erp.Application.Services.CompanyServices;

public class CompanyService : ICompanyService
{
    private readonly ErpDbContext _db;
    private readonly IMapper _mapper;

    public CompanyService(ErpDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CompanyDto> CreateCompanyAsync(CompanyCreateDto companyCreateDto)
    {
        var companyExists = await _db.Companies.AnyAsync(x => x.TaxNumber == companyCreateDto.TaxNumber && !x.IsDeleted);
        if (companyExists)
        {
            throw new CompanyTaxNumberAlreadyExistException();
        }

        var company = _mapper.Map<Company>(companyCreateDto);
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
        var companies = await _db.Companies.Where(x => !x.IsDeleted).ToListAsync();
        return _mapper.Map<List<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> GetCompanyByIdAsync(Guid id)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (company == null)
        {
            throw new NullValueException("Şirket bulunamadı");
        }
        
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task HardDeleteCompanyAsync(Guid id)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id);
        if (company == null)
        {
            throw new NullValueException("Şirket bulunamadı");
        }

        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteCompanyAsync(Guid id)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id);
        if (company == null)
        {
            throw new NullValueException("Şirket bulunamadı");
        }

        company.IsDeleted = true;
        _db.Companies.Update(company);
        await _db.SaveChangesAsync();
    }

    public async Task<CompanyDto> UpdateCompanyAsync(CompanyUpdateDto companyUpdateDto, Guid companyId)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == companyId && !x.IsDeleted);
        if (company == null)
        {
            throw new NullValueException("Şirket bulunamadı");
        }

        var existingCompany = await _db.Companies.FirstOrDefaultAsync(x => 
            x.TaxNumber == companyUpdateDto.TaxNumber && 
            x.Id != companyId && 
            !x.IsDeleted);
            
        if (existingCompany != null)
        {
            throw new Exception("Bu vergi numarası ile kayıtlı başka bir şirket bulunmaktadır.");
        }

        _mapper.Map(companyUpdateDto, company);
        company.UpdatedAt = DateTime.UtcNow;
        
        _db.Companies.Update(company);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CustomPagedResult<CompanyDto>> GetPagedAsync(PaginationRequest paginationRequest)
    {
        var query = _db.Companies.Where(x => !x.IsDeleted).AsQueryable();
        
        if (!string.IsNullOrEmpty(paginationRequest.Search))
        {
            query = query.Where(x => 
                x.Name.Contains(paginationRequest.Search) || 
                x.TaxNumber.Contains(paginationRequest.Search));
        }

        var entityResult = await query.ToPagedResultAsync(paginationRequest);
        var dtos = _mapper.Map<List<CompanyDto>>(entityResult.Items);
        
		return new CustomPagedResult<CompanyDto>
        {
            Items = dtos,
            TotalCount = entityResult.TotalCount,
            TotalPages = entityResult.TotalPages,
            PageNumber = entityResult.PageNumber,
            PageSize = entityResult.PageSize
        };
    }
} 