using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Unit;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.UnitControllers;
[Authorize(Roles = Roles.CompanyAdmin)]
public class UnitController : BaseV1Controller
{
	private readonly IUnitService _unitService;

	public UnitController(IUnitService unitService)
	{
		_unitService = unitService;
	}

	[HttpPost]
	public async Task<IActionResult> CreateUnitAsync(UnitCreateDto unitCreateDto)
	{
		var unit = await _unitService.CreateUnitAsync(unitCreateDto);
		return CustomResponse(unit);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateUnitAsync(Guid id, UnitUpdateDto unitUpdateDto)
	{
		var unit = await _unitService.UpdateUnitAsync(id, unitUpdateDto);
		return CustomResponse(unit);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteUnitAsync(Guid id)
	{
		await _unitService.DeleteAsync(id);
		return CustomResponse();
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetUnitByIdAsync(Guid id)
	{
		var unit = await _unitService.GetByIdAsync(id);
		return CustomResponse(unit);
	}

	[HttpGet]
	public async Task<IActionResult> GetUnitsAsync([FromQuery] PaginationRequest paginationRequest)
	{
		var units = await _unitService.GetPagedAsync(paginationRequest);
		return CustomResponse(units);
	}

	[HttpGet("relationUnits/{unitId}")]
	public async Task<IActionResult> GetRelationUnitsAsync(Guid unitId)
	{
		var units = await _unitService.GetRelationUnits(unitId);
		return CustomResponse(units);
	}

	[HttpGet("convertUnit/{unitId}/{rawMaterialId}")]
	public async Task<IActionResult> ConvertUnitAsync(Guid unitId, Guid rawMaterialId)
	{
		var unit = await _unitService.ConvertUnit(unitId, rawMaterialId);
		return CustomResponse(unit);
	}

	[HttpGet("findRateToRoot/{unitId}")]
	public async Task<IActionResult> FindRateToRootAsync(Guid unitId)
	{
		var rate = await _unitService.FindRateToRootAsync(unitId);
		return CustomResponse(rate);
	}
}

