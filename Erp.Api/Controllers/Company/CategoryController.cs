using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Category;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Company;

[Authorize(Roles = Roles.CompanyAdmin)]
public class CategoryController : BaseV1Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return CustomResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return CustomResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.SoftDeleteCategoryAsync(id);
        return CustomResponse();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto categoryCreateDto)
    {
        var result = await _categoryService.CreateCategoryAsync(categoryCreateDto);
        return CustomResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] CategoryUpdateDto categoryUpdateDto, Guid id)
    {
        var result = await _categoryService.UpdateCategoryAsync(categoryUpdateDto, id);
        return CustomResponse(result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest paginationRequest)
    {
        var result = await _categoryService.GetPagedAsync(paginationRequest);
        return CustomResponse(result);
    }
} 