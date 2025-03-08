using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.CustomerControllers;

[Authorize(Roles = Roles.CompanyAdmin)]
public class ProductController : BaseV1Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsAsync();
        return CustomResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return CustomResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.SoftDeleteProductAsync(id);
        return CustomResponse();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto productCreateDto)
    {
        var result = await _productService.CreateProductAsync(productCreateDto);
        return CustomResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] ProductUpdateDto productUpdateDto, Guid id)
    {
        var result = await _productService.UpdateProductAsync(productUpdateDto, id);
        return CustomResponse(result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest paginationRequest)
    {
        var result = await _productService.GetPagedAsync(paginationRequest);
        return CustomResponse(result);
    }
} 