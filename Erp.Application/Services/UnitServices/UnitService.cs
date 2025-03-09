using AutoMapper;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Unit;
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
using Erp.Domain.DTOs.Category;

namespace Erp.Application.Services.UnitServices;

public class UnitService : IUnitService
{
	private readonly ErpDbContext _db;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILocalizationService _localizationService;
	private readonly IMapper _mapper;
	public UnitService(ErpDbContext context, ILocalizationService localizationService, ICurrentUserService currentUserService, IMapper mapper)
	{
		_db = context;
		_localizationService = localizationService;
		_currentUserService = currentUserService;
		_mapper = mapper;
	}

	public async Task<UnitDto> CreateUnitAsync(UnitCreateDto unitCreateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}
		var nameExists = await _db.Units.AnyAsync(
			x => x.Name.ToLower() == unitCreateDto.Name.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value || x.IsGlobal) &&
			!x.IsDeleted);
		if (nameExists)
		{
			throw new UnitNameAlreadyExistsException(_localizationService);
		}

		var shortCodeExists = await _db.Units.AnyAsync(
			x => x.ShortCode.ToLower() == unitCreateDto.ShortCode.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value || x.IsGlobal) &&
			!x.IsDeleted);

		if (shortCodeExists)
		{
			throw new UnitShortCodeAlreadyExistsException(_localizationService);
		}

		var unit = _mapper.Map<Unit>(unitCreateDto);
		unit.CompanyId = currentUser.CompanyId;
		var rootUnit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unitCreateDto.RootUnitId);
		unit.UnitType = rootUnit.UnitType;
		var rootUnitRate = await FindRateToRootAsync(rootUnit.Id);
		unit.RateToRoot = rootUnitRate * unit.ConversionRate;
		await _db.Units.AddAsync(unit);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UnitDto>(unit);
		return dto;
	}

	public async Task<UnitDto> UpdateUnitAsync(Guid id, UnitUpdateDto unitUpdateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == id);
		if (unit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		if (unit.CompanyId != currentUser.CompanyId || unit.IsGlobal)
		{
			throw new UnauthorizedAccessException();
		}
		var nameExists = await _db.Units.AnyAsync(
			x => x.Name.ToLower() == unitUpdateDto.Name.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value || x.IsGlobal) &&
			!x.IsDeleted &&
			x.Id != id);
		if (nameExists)
		{
			throw new UnitNameAlreadyExistsException(_localizationService);
		}
		var shortCodeExists = await _db.Units.AnyAsync(
			x => x.ShortCode.ToLower() == unitUpdateDto.ShortCode.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value || x.IsGlobal) &&
			!x.IsDeleted &&
			x.Id != id);
		if (shortCodeExists)
		{
			throw new UnitShortCodeAlreadyExistsException(_localizationService);
		}
		var result = _mapper.Map(unitUpdateDto, unit);
		var rootUnit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unitUpdateDto.RootUnitId);
		result.UnitType = rootUnit.UnitType;
		var rootUnitRate = await FindRateToRootAsync(rootUnit.Id);
		unit.RateToRoot = rootUnitRate * unit.ConversionRate;
		_db.Units.Update(result);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UnitDto>(result);
		return dto;
	}

	public async Task DeleteAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsGlobal);
		if (unit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		var productRawMaterialExists = await _db.RawMaterials.AnyAsync(x => x.UnitId == id && !x.IsDeleted);
		if (productRawMaterialExists)
		{
			throw new UnitHasProductRawMaterialException(_localizationService);
		}
		var unitExists = await _db.Units.AnyAsync(x => x.RootUnitId == id && !x.IsDeleted);
		if (unitExists)
		{
			throw new UnitHasChildUnitException(_localizationService);
		}
		var productFormulationExists = await _db.ProductFormulaItems.AnyAsync(x => x.UnitId == id && !x.IsDeleted && !x.ProductFormula.IsDeleted);
		if (productFormulationExists)
		{
			throw new UnitHasProductFormulationException(_localizationService);
		}
		unit.IsDeleted = true;
		_db.Units.Update(unit);
		await _db.SaveChangesAsync();
	}

	public async Task<UnitDto> GetByIdAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (unit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		var result = _mapper.Map<UnitDto>(unit);
		return result;
	}

	public async Task<CustomPagedResult<UnitDto>> GetPagedAsync(PaginationRequest paginationRequest)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var query = _db.Units.Where(x => (x.CompanyId == currentUser.CompanyId || x.IsGlobal) && !x.IsDeleted);
		if (!string.IsNullOrEmpty(paginationRequest.Search))
        {
            query = query.Where(x => 
                x.Name.ToLower().Contains(paginationRequest.Search.ToLower()) || 
                (x.Description.ToLower().Contains(paginationRequest.Search.ToLower())));
        }
		if (!string.IsNullOrEmpty(paginationRequest.Query))
		{
			query = query.Where(paginationRequest.Query);
		}
		var entityResult = await query.ToPagedResultAsync(paginationRequest);
		var dtos = _mapper.Map<List<UnitDto>>(entityResult.Items);

		return new CustomPagedResult<UnitDto>
		{
			Items = dtos,
			TotalCount = entityResult.TotalCount,
			TotalPages = entityResult.TotalPages,
			PageNumber = entityResult.PageNumber,
			PageSize = entityResult.PageSize
		};
	}

	public async Task<List<UnitDto>> GetRelationUnits(Guid unitId)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unitId && (x.CompanyId == currentUser.CompanyId || x.IsGlobal) && !x.IsDeleted);
		var query = _db.Units.Where(o => !o.IsDeleted && (o.CompanyId == currentUser.CompanyId || o.IsGlobal)
		&& o.UnitType == unit.UnitType);
		var entityResult = await query.ToListAsync();
		var dtos = _mapper.Map<List<UnitDto>>(entityResult);
		return dtos;
	}

	public async Task<decimal> FindRateToRootAsync(Guid unitId)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unitId && (x.CompanyId == currentUser.CompanyId || x.IsGlobal) && !x.IsDeleted);
		if (unit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		var rate = 1m;
		while (unit.RootUnitId != null)
		{
			rate *= unit.ConversionRate;
			unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unit.RootUnitId);
		}
		return rate;
	}

	public async Task<decimal> ConvertUnit(Guid unitId, Guid rawMaterialId)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var rawMaterial = await _db.RawMaterials.FirstOrDefaultAsync(x => x.Id == rawMaterialId && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (rawMaterial == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.RawMaterialNotFound));
		}
		var rate = 1m;
		if (rawMaterial.UnitId == unitId)
		{
			return rate;
		}
		
		// Calculate conversion rate between units
		// Get the source unit (raw material's unit)
		var sourceUnit = await _db.Units.FirstOrDefaultAsync(x => x.Id == rawMaterial.UnitId && !x.IsDeleted);
		if (sourceUnit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		
		// Get the target unit
		var targetUnit = await _db.Units.FirstOrDefaultAsync(x => x.Id == unitId && !x.IsDeleted);
		if (targetUnit == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UnitNotFound));
		}
		
		// Check if units are of the same type
		if (sourceUnit.UnitType != targetUnit.UnitType)
		{
			throw new UnitTypeMismatchException(_localizationService);
		}
		
		// Calculate conversion rates to root unit for both source and target
		//var sourceToRootRate = await FindRateToRootAsync(sourceUnit.Id);
		//var targetToRootRate = await FindRateToRootAsync(targetUnit.Id);
		
		// Calculate the conversion rate from source to target
		// If source is 5 times bigger than root, and target is 2 times bigger than root
		// Then source is 5/2 = 2.5 times bigger than target
		rate = Math.Round(sourceUnit.RateToRoot / targetUnit.RateToRoot , 3);

		return rate;
	}
}
