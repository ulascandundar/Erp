using AutoMapper;
using Erp.Application.Common.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace Erp.Application.Services.RawMaterialServices;

public class RawMaterialService : IRawMaterialService
{
	private readonly ErpDbContext _db;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILocalizationService _localizationService;
	private readonly IMapper _mapper;
	private readonly IUnitService _unitService;
	
	public RawMaterialService(
		ErpDbContext context, 
		ILocalizationService localizationService, 
		ICurrentUserService currentUserService, 
		IMapper mapper, 
		IUnitService unitService)
	{
		_db = context;
		_localizationService = localizationService;
		_currentUserService = currentUserService;
		_mapper = mapper;
		_unitService = unitService;
	}

	public async Task<RawMaterialDto> CreateRawMaterialAsync(RawMaterialCreateDto rawMaterialCreateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}
		var nameExists = await _db.RawMaterials.AnyAsync(
			x => x.Name.ToLower() == rawMaterialCreateDto.Name.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value) &&
			!x.IsDeleted);
		if (nameExists)
		{
			throw new RawMaterialNameAlreadyExistsException(_localizationService);
		}

		var barcodeExists = await _db.RawMaterials.AnyAsync(
			x => x.Barcode == rawMaterialCreateDto.Barcode &&
			(x.CompanyId == currentUser.CompanyId.Value) &&
			!x.IsDeleted);
		if (barcodeExists)
		{
			throw new RawMaterialBarcodeAlreadyExistsException(_localizationService);
		}
		var rawMaterial = _mapper.Map<RawMaterial>(rawMaterialCreateDto);
		rawMaterial.CompanyId = currentUser.CompanyId.Value;
		await _db.RawMaterials.AddAsync(rawMaterial);
		await _db.SaveChangesAsync();
		
		// Add raw material to multiple suppliers if specified
		if (rawMaterialCreateDto.SupplierIds != null && rawMaterialCreateDto.SupplierIds.Any())
		{
			foreach (var supplierId in rawMaterialCreateDto.SupplierIds)
			{
				// Check if supplier exists
				var supplier = await _db.Suppliers
					.FirstOrDefaultAsync(s => s.Id == supplierId && s.CompanyId == currentUser.CompanyId && !s.IsDeleted);
					
				if (supplier == null)
				{
					continue; // Skip if supplier doesn't exist
				}
				
				// Check if association already exists
				var existingAssociation = await _db.RawMaterialSuppliers
					.FirstOrDefaultAsync(rms => rms.SupplierId == supplierId && 
											   rms.RawMaterialId == rawMaterial.Id && 
											   !rms.IsDeleted);
											   
				if (existingAssociation != null)
				{
					continue; // Skip if association already exists
				}
				
				// Create new association
				var rawMaterialSupplier = new RawMaterialSupplier
				{
					SupplierId = supplierId,
					RawMaterialId = rawMaterial.Id,
					Price = rawMaterial.Price // Use raw material price as default
				};
				
				await _db.RawMaterialSuppliers.AddAsync(rawMaterialSupplier);
			}
			
			await _db.SaveChangesAsync();
		}
		
		return _mapper.Map<RawMaterialDto>(rawMaterial);
	}

	public async Task<RawMaterialDto> UpdateRawMaterialAsync(Guid id, RawMaterialUpdateDto rawMaterialUpdateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var rawMaterial = await _db.RawMaterials.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (rawMaterial == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.RawMaterialNotFound));
		}
		var nameExists = await _db.RawMaterials.AnyAsync(
			x => x.Name.ToLower() == rawMaterialUpdateDto.Name.ToLower() &&
			(x.CompanyId == currentUser.CompanyId.Value) &&
			!x.IsDeleted &&
			x.Id != id);
		if (nameExists)
		{
			throw new RawMaterialNameAlreadyExistsException(_localizationService);
		}
		var barcodeExists = await _db.RawMaterials.AnyAsync(
			x => x.Barcode == rawMaterialUpdateDto.Barcode &&
			(x.CompanyId == currentUser.CompanyId.Value) &&
			!x.IsDeleted &&
			x.Id != id);
		if (barcodeExists)
		{
			throw new RawMaterialBarcodeAlreadyExistsException(_localizationService);
		}
		rawMaterial.Barcode = rawMaterialUpdateDto.Barcode;
		rawMaterial.Name = rawMaterialUpdateDto.Name;
		rawMaterial.Description = rawMaterialUpdateDto.Description;
		rawMaterial.Price = rawMaterialUpdateDto.Price;
		var newRate = await _unitService.ConvertUnit(rawMaterialUpdateDto.UnitId, rawMaterial.Id);
		rawMaterial.Stock = Math.Round(rawMaterial.Stock * newRate, 3);
		rawMaterial.UnitId = rawMaterialUpdateDto.UnitId;
		await _db.SaveChangesAsync();
		
		// Handle supplier associations if specified
		if (rawMaterialUpdateDto.SupplierIds != null && rawMaterialUpdateDto.SupplierIds.Any())
		{
			// Get current supplier associations
			var currentSupplierRawMaterials = await _db.RawMaterialSuppliers
				.Where(x => x.RawMaterialId == id && !x.IsDeleted)
				.ToListAsync();
			
			var currentSupplierIds = currentSupplierRawMaterials.Select(x => x.SupplierId).ToList();
			
			// Add new associations
			foreach (var supplierId in rawMaterialUpdateDto.SupplierIds)
			{
				if (!currentSupplierIds.Contains(supplierId))
				{
					// Check if supplier exists
					var supplier = await _db.Suppliers
						.FirstOrDefaultAsync(s => s.Id == supplierId && s.CompanyId == currentUser.CompanyId && !s.IsDeleted);
						
					if (supplier == null)
					{
						continue; // Skip if supplier doesn't exist
					}
					
					// Create new association
					var rawMaterialSupplier = new RawMaterialSupplier
					{
						SupplierId = supplierId,
						RawMaterialId = id,
						Price = rawMaterial.Price // Use raw material price as default
					};
					
					await _db.RawMaterialSuppliers.AddAsync(rawMaterialSupplier);
				}
			}
			
			await _db.SaveChangesAsync();
		}
		
		return _mapper.Map<RawMaterialDto>(rawMaterial);
	}

	public async Task<RawMaterialDto> GetRawMaterialAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var rawMaterial = await _db.RawMaterials.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (rawMaterial == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.RawMaterialNotFound));
		}
		return _mapper.Map<RawMaterialDto>(rawMaterial);
	}

	public async Task<CustomPagedResult<RawMaterialDto>> GetRawMaterialsAsync(PaginationRequest paginationRequest)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var query = _db.RawMaterials.Where(x => x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (!string.IsNullOrEmpty(paginationRequest.Query))
		{
			query = query.Where(paginationRequest.Query);
		}
		if (!string.IsNullOrEmpty(paginationRequest.Search))
		{
			query = query.Where(o => o.Name.ToLower().Contains(paginationRequest.Search.ToLower())
				|| o.Barcode.ToLower().Contains(paginationRequest.Search.ToLower()));
		}
		var entityResult = await query.ToPagedResultAsync(paginationRequest);
		var dtos = _mapper.Map<List<RawMaterialDto>>(entityResult.Items);
		return new CustomPagedResult<RawMaterialDto>
		{
			Items = dtos,
			TotalCount = entityResult.TotalCount,
			TotalPages = entityResult.TotalPages,
			PageNumber = entityResult.PageNumber,
			PageSize = entityResult.PageSize
		};
	}

	public async Task DeleteRawMaterialAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var rawMaterial = await _db.RawMaterials.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (rawMaterial == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.RawMaterialNotFound));
		}
		var rawMaterialExistFormula = await _db.ProductFormulaItems
		.Where(x => x.RawMaterialId == id && !x.IsDeleted && !x.ProductFormula.IsDeleted).AnyAsync();
		if (rawMaterialExistFormula)
		{
			throw new RawMaterialExistInFormulaException(_localizationService);
		}
		rawMaterial.IsDeleted = true;
		_db.RawMaterials.Update(rawMaterial);
		await _db.SaveChangesAsync();
	}
}
