using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.ProductFormula;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.ProductFormulaControllers;

[Authorize(Roles = Roles.CompanyAdmin)]
public class ProductFormulaController : BaseV1Controller
{
	private readonly IProductFormulaService _productFormulaService;

	public ProductFormulaController(IProductFormulaService productFormulaService)
	{
		_productFormulaService = productFormulaService;
	}

	[HttpPost]
	public async Task<IActionResult> CreateProductFormula(ProductFormulaCreateDto productFormulaCreateDto)
	{
		var productFormula = await _productFormulaService.CreateProductFormulaAsync(productFormulaCreateDto);
		return CustomResponse(productFormula);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateProductFormula(Guid id, ProductFormulaUpdateDto productFormulaUpdateDto)
	{
		var productFormula = await _productFormulaService.UpdateProductFormulaAsync(id, productFormulaUpdateDto);
		return CustomResponse(productFormula);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteProductFormula(Guid id)
	{
		await _productFormulaService.DeleteProductFormulaAsync(id);
		return CustomResponse(true);
	}
	
	[HttpGet]
	public async Task<IActionResult> GetProductFormulas([FromQuery] PaginationRequest paginationRequest)
	{
		var productFormulas = await _productFormulaService.GetProductFormulasAsync(paginationRequest);
		return CustomResponse(productFormulas);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetProductFormula(Guid id)
	{
		var productFormula = await _productFormulaService.GetProductFormulaAsync(id);
		return CustomResponse(productFormula);
	}

}
