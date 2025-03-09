using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.RawMaterialControllers;

[Authorize(Roles = Roles.CompanyAdmin)]
public class RawMaterialController : BaseV1Controller
{
	private readonly IRawMaterialService _rawMaterialService;

	public RawMaterialController(IRawMaterialService rawMaterialService)
	{
		_rawMaterialService = rawMaterialService;
	}

	[HttpPost]
	public async Task<IActionResult> CreateRawMaterial([FromBody] RawMaterialCreateDto rawMaterialCreateDto)
	{
		var rawMaterial = await _rawMaterialService.CreateRawMaterialAsync(rawMaterialCreateDto);
		return CustomResponse(rawMaterial);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateRawMaterial(Guid id, [FromBody] RawMaterialUpdateDto rawMaterialUpdateDto)
	{
		var rawMaterial = await _rawMaterialService.UpdateRawMaterialAsync(id, rawMaterialUpdateDto);
		return CustomResponse(rawMaterial);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetRawMaterial(Guid id)
	{
		var rawMaterial = await _rawMaterialService.GetRawMaterialAsync(id);
		return CustomResponse(rawMaterial);
	}
	[HttpGet]
	public async Task<IActionResult> GetRawMaterials([FromQuery] PaginationRequest paginationRequest)
	{
		var rawMaterials = await _rawMaterialService.GetRawMaterialsAsync(paginationRequest);
		return CustomResponse(rawMaterials);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteRawMaterial(Guid id)
	{
		await _rawMaterialService.DeleteRawMaterialAsync(id);
		return CustomResponse();
	}
}