using AutoMapper;
using Erp.Application.Common.Extensions;
using Erp.Application.Validators.CategoryValidator;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Category;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Application.Services.CategoryServices;

public class CategoryService : ICategoryService
{
    private readonly ErpDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;

    public CategoryService(
        ErpDbContext db, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        ILocalizationService localizationService)
    {
        _db = db;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizationService = localizationService;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto categoryCreateDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        // Veritabanı kontrolleri
        // Aynı şirkette aynı isimde kategori var mı?
        var nameExists = await _db.Categories.AnyAsync(
            x => x.Name == categoryCreateDto.Name && 
            x.CompanyId == currentUser.CompanyId.Value && 
            !x.IsDeleted);
            
        if (nameExists)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNameAlreadyExists));
        }

        var category = _mapper.Map<Category>(categoryCreateDto);
        category.CompanyId = currentUser.CompanyId.Value;
        
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var categories = await _db.Categories
            .Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
            .ToListAsync();
            
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var category = await _db.Categories.FirstOrDefaultAsync(
            x => x.Id == id && 
            x.CompanyId == currentUser.CompanyId.Value && 
            !x.IsDeleted);
            
        if (category == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNotFound));
        }
        
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task HardDeleteCategoryAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var category = await _db.Categories.FirstOrDefaultAsync(
            x => x.Id == id && 
            x.CompanyId == currentUser.CompanyId.Value);
            
        if (category == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNotFound));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteCategoryAsync(Guid id)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var category = await _db.Categories.FirstOrDefaultAsync(
            x => x.Id == id && 
            x.CompanyId == currentUser.CompanyId.Value && 
            !x.IsDeleted);
            
        if (category == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNotFound));
        }

        category.IsDeleted = true;
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task<CategoryDto> UpdateCategoryAsync(CategoryUpdateDto categoryUpdateDto, Guid categoryId)
    {

        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var category = await _db.Categories.FirstOrDefaultAsync(
            x => x.Id == categoryId && 
            x.CompanyId == currentUser.CompanyId.Value && 
            !x.IsDeleted);
            
        if (category == null)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNotFound));
        }

        // Veritabanı kontrolleri
        // Aynı şirkette aynı isimde başka kategori var mı? (kendisi hariç)
        var nameExists = await _db.Categories.AnyAsync(
            x => x.Name == categoryUpdateDto.Name && 
            x.CompanyId == currentUser.CompanyId.Value && 
            x.Id != categoryId && 
            !x.IsDeleted);
            
        if (nameExists)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.CategoryNameAlreadyExists));
        }

        _mapper.Map(categoryUpdateDto, category);
        category.UpdatedAt = DateTime.UtcNow;
        
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CustomPagedResult<CategoryDto>> GetPagedAsync(PaginationRequest paginationRequest)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        var query = _db.Categories
            .Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(paginationRequest.Search))
        {
            query = query.Where(x => 
                x.Name.Contains(paginationRequest.Search) || 
                (x.Description != null && x.Description.Contains(paginationRequest.Search)));
        }

        var entityResult = await query.ToPagedResultAsync(paginationRequest);
        var dtos = _mapper.Map<List<CategoryDto>>(entityResult.Items);
        
        return new CustomPagedResult<CategoryDto>
        {
            Items = dtos,
            TotalCount = entityResult.TotalCount,
            TotalPages = entityResult.TotalPages,
            PageNumber = entityResult.PageNumber,
            PageSize = entityResult.PageSize
        };
    }
} 