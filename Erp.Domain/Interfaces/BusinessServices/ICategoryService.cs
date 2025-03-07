using Erp.Domain.DTOs.Category;
using Erp.Domain.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto categoryCreateDto);
    Task<CategoryDto> UpdateCategoryAsync(CategoryUpdateDto categoryUpdateDto, Guid categoryId);
    Task HardDeleteCategoryAsync(Guid id);
    Task SoftDeleteCategoryAsync(Guid id);
    Task<CustomPagedResult<CategoryDto>> GetPagedAsync(PaginationRequest paginationRequest);
    Task<CategoryWithProductsDto> GetCategoryWithProductsByIdAsync(Guid id);
} 