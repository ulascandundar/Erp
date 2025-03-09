using AutoMapper;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.ProductFormula;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Erp.Application.Common.Extensions;

namespace Erp.Application.Services.ProductFormulaServices;

public class ProductFormulaService : IProductFormulaService
{
    private readonly ErpDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public ProductFormulaService(ErpDbContext context, ILocalizationService localizationService, ICurrentUserService currentUserService, IMapper mapper)
    {
        _db = context;
        _localizationService = localizationService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ProductFormulaDto> CreateProductFormulaAsync(ProductFormulaCreateDto productFormulaCreateDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        // Check if formula name already exists for this company
        var nameExists = await _db.ProductFormulas.AnyAsync(
            x => x.Name.ToLower() == productFormulaCreateDto.Name.ToLower() &&
            x.CompanyId == currentUser.CompanyId.Value &&
            !x.IsDeleted);

        if (nameExists)
        {
            throw new ProductFormulaNameAlreadyExistsException(_localizationService);
        }

        // Create new formula
        var productFormula = _mapper.Map<ProductFormula>(productFormulaCreateDto);
        productFormula.CompanyId = currentUser.CompanyId.Value;

        await _db.ProductFormulas.AddAsync(productFormula);
        await _db.SaveChangesAsync();

        return await GetProductFormulaAsync(productFormula.Id);
    }

    public async Task<ProductFormulaDto> UpdateProductFormulaAsync(Guid id, ProductFormulaUpdateDto productFormulaUpdateDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        // Get formula with items
        var productFormula = await _db.ProductFormulas
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && 
                                x.CompanyId == currentUser.CompanyId && 
                                !x.IsDeleted);

        if (productFormula == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductFormulaNotFound));
        }

        // Check if formula name already exists for this company (excluding current formula)
        var nameExists = await _db.ProductFormulas.AnyAsync(
            x => x.Name.ToLower() == productFormulaUpdateDto.Name.ToLower() &&
            x.CompanyId == currentUser.CompanyId.Value &&
            x.Id != id &&
            !x.IsDeleted);

        if (nameExists)
        {
            throw new ProductFormulaNameAlreadyExistsException(_localizationService);
        }

        // Update formula properties
        productFormula.Name = productFormulaUpdateDto.Name;
        productFormula.Description = productFormulaUpdateDto.Description;

        // Handle formula items
        var existingItemIds = productFormula.Items.Where(x => !x.IsDeleted).Select(x => x.Id).ToList();
        var updatedItemIds = productFormulaUpdateDto.Items.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();

        // Items to delete (exist in DB but not in update DTO)
        var itemsToDelete = existingItemIds.Except(updatedItemIds).ToList();
        foreach (var itemId in itemsToDelete)
        {
            var item = productFormula.Items.First(x => x.Id == itemId);
            _db.ProductFormulaItems.Remove(item);
        }

        // Update existing items and add new ones
        foreach (var itemDto in productFormulaUpdateDto.Items)
        {
            // Verify raw material exists and belongs to the company
            var rawMaterial = await _db.RawMaterials.FirstOrDefaultAsync(
                x => x.Id == itemDto.RawMaterialId && 
                x.CompanyId == currentUser.CompanyId.Value && 
                !x.IsDeleted);

            if (rawMaterial == null)
            {
                throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.RawMaterialNotFound));
            }

            // Verify unit exists
            var unit = await _db.Units.FirstOrDefaultAsync(
                x => x.Id == itemDto.UnitId && 
                (x.CompanyId == currentUser.CompanyId.Value || x.IsGlobal) && 
                !x.IsDeleted);

            if (unit == null)
            {
                throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
            }

            if (itemDto.Id.HasValue)
            {
                // Update existing item
                var existingItem = productFormula.Items.FirstOrDefault(x => x.Id == itemDto.Id.Value && !x.IsDeleted);
                if (existingItem != null)
                {
                    existingItem.RawMaterialId = itemDto.RawMaterialId;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.UnitId = itemDto.UnitId;
                }
            }
            else
            {
                // Add new item
                var newItem = new ProductFormulaItem
                {
                    RawMaterialId = itemDto.RawMaterialId,
                    Quantity = itemDto.Quantity,
                    UnitId = itemDto.UnitId
                };
                productFormula.Items.Add(newItem);
            }
        }

        _db.ProductFormulas.Update(productFormula);
        await _db.SaveChangesAsync();

        return await GetProductFormulaAsync(productFormula.Id);
    }

    public async Task DeleteProductFormulaAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var productFormula = await _db.ProductFormulas
            .FirstOrDefaultAsync(x => x.Id == id && 
                                x.CompanyId == currentUser.CompanyId && 
                                !x.IsDeleted);

        if (productFormula == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductFormulaNotFound));
        }

        // Check if formula is used by any products
        var isUsedByProducts = await _db.Products
            .AnyAsync(x => x.ProductFormulaId == id && !x.IsDeleted);

        if (isUsedByProducts)
        {
            throw new ProductFormulaUsedByProductException(_localizationService);
        }

        // Soft delete the formula and its items
        productFormula.IsDeleted = true;
        
        // Also mark all items as deleted
        var formulaItems = await _db.ProductFormulaItems
            .Where(x => x.ProductFormulaId == id && !x.IsDeleted)
            .ToListAsync();
            
        foreach (var item in formulaItems)
        {
            item.IsDeleted = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ProductFormulaDto> GetProductFormulaAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var productFormula = await _db.ProductFormulas
            .Include(x => x.Items.Where(i => !i.IsDeleted))
                .ThenInclude(x => x.RawMaterial)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
                .ThenInclude(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == id && 
                                x.CompanyId == currentUser.CompanyId && 
                                !x.IsDeleted);

        if (productFormula == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductFormulaNotFound));
        }

        return _mapper.Map<ProductFormulaDto>(productFormula);
    }

    public async Task<CustomPagedResult<ProductFormulaDto>> GetProductFormulasAsync(PaginationRequest paginationRequest)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var query = _db.ProductFormulas
            .Where(x => x.CompanyId == currentUser.CompanyId && !x.IsDeleted)
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(paginationRequest.Search))
        {
            var searchTerm = paginationRequest.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(searchTerm) || 
                                    x.Description.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(paginationRequest.Query))
        {
            query = query.Where(paginationRequest.Query);
        }

        // Execute pagination
        var entityResult = await query.ToPagedResultAsync(paginationRequest);
        
        // Map to DTOs
        var dtos = _mapper.Map<List<ProductFormulaDto>>(entityResult.Items);
        
        return new CustomPagedResult<ProductFormulaDto>
        {
            Items = dtos,
            TotalCount = entityResult.TotalCount,
            TotalPages = entityResult.TotalPages,
            PageNumber = entityResult.PageNumber,
            PageSize = entityResult.PageSize
        };
    }
} 