using AutoMapper;
using Erp.Application.Common.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Supplier;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Services.SupplierServices;

public class SupplierService : ISupplierService
{
    private readonly ErpDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public SupplierService(
        ErpDbContext db,
        ICurrentUserService currentUserService,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _db = db;
        _currentUserService = currentUserService;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto supplierCreateDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        // Check if supplier with same name already exists
        var nameExists = await _db.Suppliers.AnyAsync(
            x => x.Name.ToLower() == supplierCreateDto.Name.ToLower() &&
            x.CompanyId == currentUser.CompanyId.Value &&
            !x.IsDeleted);

        if (nameExists)
        {
            throw new SupplierNameAlreadyExistsException();
        }

        // Check if supplier with same tax number already exists
        if (!string.IsNullOrEmpty(supplierCreateDto.TaxNumber))
        {
            var taxNumberExists = await _db.Suppliers.AnyAsync(
                x => x.TaxNumber == supplierCreateDto.TaxNumber &&
                x.CompanyId == currentUser.CompanyId.Value &&
                !x.IsDeleted);

            if (taxNumberExists)
            {
                throw new SupplierTaxNumberAlreadyExistsException();
            }
        }

        var supplier = _mapper.Map<Supplier>(supplierCreateDto);
        supplier.CompanyId = currentUser.CompanyId.Value;

        await _db.Suppliers.AddAsync(supplier);
        await _db.SaveChangesAsync();

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(Guid id, SupplierUpdateDto supplierUpdateDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(
            x => x.Id == id &&
            x.CompanyId == currentUser.CompanyId &&
            !x.IsDeleted);

        if (supplier == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.SupplierNotFound));
        }

        // Check if supplier with same name already exists (excluding current supplier)
        var nameExists = await _db.Suppliers.AnyAsync(
            x => x.Name.ToLower() == supplierUpdateDto.Name.ToLower() &&
            x.CompanyId == currentUser.CompanyId.Value &&
            x.Id != id &&
            !x.IsDeleted);

        if (nameExists)
        {
            throw new SupplierNameAlreadyExistsException();
        }

        // Check if supplier with same tax number already exists (excluding current supplier)
        if (!string.IsNullOrEmpty(supplierUpdateDto.TaxNumber))
        {
            var taxNumberExists = await _db.Suppliers.AnyAsync(
                x => x.TaxNumber == supplierUpdateDto.TaxNumber &&
                x.CompanyId == currentUser.CompanyId.Value &&
                x.Id != id &&
                !x.IsDeleted);

            if (taxNumberExists)
            {
                throw new SupplierTaxNumberAlreadyExistsException();
            }
        }

        _mapper.Map(supplierUpdateDto, supplier);
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task DeleteSupplierAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(
            x => x.Id == id &&
            x.CompanyId == currentUser.CompanyId &&
            !x.IsDeleted);

        if (supplier == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.SupplierNotFound));
        }

        // Check if supplier has raw materials
        var hasRawMaterials = await _db.RawMaterialSuppliers.AnyAsync(
            x => x.SupplierId == id && !x.IsDeleted);

        if (hasRawMaterials)
        {
            throw new SupplierHasRawMaterialsException();
        }

        supplier.IsDeleted = true;
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task<SupplierDto> GetSupplierByIdAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var supplier = await _db.Suppliers
            .Include(x => x.RawMaterialSuppliers.Where(rms => !rms.IsDeleted))
                .ThenInclude(x => x.RawMaterial)
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                x.CompanyId == currentUser.CompanyId &&
                !x.IsDeleted);

        if (supplier == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.SupplierNotFound));
        }

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<CustomPagedResult<SupplierDto>> GetSuppliersAsync(PaginationRequest paginationRequest)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var query = _db.Suppliers
            .Where(x => x.CompanyId == currentUser.CompanyId && !x.IsDeleted)
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(paginationRequest.Search))
        {
            var searchTerm = paginationRequest.Search.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(searchTerm) || 
                x.ContactPerson.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                x.PhoneNumber.Contains(searchTerm) ||
                x.TaxNumber.Contains(searchTerm));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(paginationRequest.OrderBy))
        {
            query = query.OrderBy($"{paginationRequest.OrderBy} {(paginationRequest.IsDesc ? "desc" : "asc")}");
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        // Execute pagination
        var entityResult = await query.ToPagedResultAsync(paginationRequest);
        
        // Map to DTOs
        var dtos = _mapper.Map<List<SupplierDto>>(entityResult.Items);
        
        return new CustomPagedResult<SupplierDto>
        {
            Items = dtos,
            TotalCount = entityResult.TotalCount,
            TotalPages = entityResult.TotalPages,
            PageNumber = entityResult.PageNumber,
            PageSize = entityResult.PageSize
        };
    }
} 