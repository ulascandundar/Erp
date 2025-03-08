using AutoMapper;
using Erp.Application.Common.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Erp.Application.Services.ProductServices;

public class ProductService : IProductService
{
	private readonly ErpDbContext _db;
	private readonly IMapper _mapper;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILocalizationService _localizationService;

	public ProductService(
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

	public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		// Veritabanı kontrolleri
		// Aynı şirkette aynı isimde ürün var mı?
		var nameExists = await _db.Products.AnyAsync(
			x => x.Name == productCreateDto.Name &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (nameExists)
		{
			throw new ProductNameAlreadyExistsException();
		}

		// Aynı şirkette aynı SKU var mı?
		var skuExists = await _db.Products.AnyAsync(
			x => x.SKU == productCreateDto.SKU &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (skuExists)
		{
			throw new SkuAlreadyExistsException();
		}

		var barcodeExists = await _db.Products.AnyAsync(
			x => x.Barcode == productCreateDto.Barcode &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (barcodeExists)
		{
			throw new ProductBarcodeAlreadyExistsException();
		}

		var product = _mapper.Map<Product>(productCreateDto);
		product.CompanyId = currentUser.CompanyId.Value;

		await _db.Products.AddAsync(product);
		await _db.SaveChangesAsync();

		return _mapper.Map<ProductDto>(product);
	}

	public async Task<List<ProductDto>> GetAllProductsAsync()
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var products = await _db.Products
			.Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
			.ToListAsync();

		return _mapper.Map<List<ProductDto>>(products);
	}

	public async Task<ProductDto> GetProductByIdAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var product = await _db.Products.FirstOrDefaultAsync(
			x => x.Id == id &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (product == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
		}

		return _mapper.Map<ProductDto>(product);
	}

	public async Task HardDeleteProductAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var product = await _db.Products.FirstOrDefaultAsync(
			x => x.Id == id &&
			x.CompanyId == currentUser.CompanyId.Value);

		if (product == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
		}

		_db.Products.Remove(product);
		await _db.SaveChangesAsync();
	}

	public async Task SoftDeleteProductAsync(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var product = await _db.Products.FirstOrDefaultAsync(
			x => x.Id == id &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (product == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
		}

		product.IsDeleted = true;
		_db.Products.Update(product);
		await _db.SaveChangesAsync();
	}

	public async Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto, Guid productId)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var product = await _db.Products.Include(o=>o.ProductCategories).FirstOrDefaultAsync(
			x => x.Id == productId &&
			x.CompanyId == currentUser.CompanyId.Value &&
			!x.IsDeleted);

		if (product == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
		}

		// Veritabanı kontrolleri
		// Aynı şirkette aynı isimde başka ürün var mı? (kendisi hariç)
		var nameExists = await _db.Products.AnyAsync(
			x => x.Name == productUpdateDto.Name &&
			x.CompanyId == currentUser.CompanyId.Value &&
			x.Id != productId &&
			!x.IsDeleted);

		if (nameExists)
		{
			throw new ProductNameAlreadyExistsException();
		}

		// Aynı şirkette aynı SKU'da başka ürün var mı? (kendisi hariç)
		var skuExists = await _db.Products.AnyAsync(
			x => x.SKU == productUpdateDto.SKU &&
			x.CompanyId == currentUser.CompanyId.Value &&
			x.Id != productId &&
			!x.IsDeleted);

		if (skuExists)
		{
			throw new SkuAlreadyExistsException();
		}

		var barcodeExists = await _db.Products.AnyAsync(
			x => x.Barcode == productUpdateDto.Barcode &&
			x.CompanyId == currentUser.CompanyId.Value &&
			x.Id != productId &&
			!x.IsDeleted);

		if (barcodeExists)
		{
			throw new ProductBarcodeAlreadyExistsException();
		}

		_mapper.Map(productUpdateDto, product);
		product.UpdatedAt = DateTime.UtcNow;

		_db.Products.Update(product);
		await _db.SaveChangesAsync();

		return _mapper.Map<ProductDto>(product);
	}

	public async Task<CustomPagedResult<ProductDto>> GetPagedAsync(PaginationRequest paginationRequest)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var query = _db.Products
			.Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
			.AsQueryable();

		if (!string.IsNullOrEmpty(paginationRequest.Search))
		{
			query = query.Where(x =>
				x.Name.Contains(paginationRequest.Search) ||
				x.SKU.Contains(paginationRequest.Search) ||
				x.Barcode.Contains(paginationRequest.Search));
		}
		query = query.Include(x => x.ProductCategories).ThenInclude(o=>o.Category);
		if (!string.IsNullOrEmpty(paginationRequest.Query))
		{
			query = query.Where(paginationRequest.Query);
		}
		var entityResult = await query.ToPagedResultAsync(paginationRequest);
		var dtos = _mapper.Map<List<ProductDto>>(entityResult.Items);

		return new CustomPagedResult<ProductDto>
		{
			Items = dtos,
			TotalCount = entityResult.TotalCount,
			TotalPages = entityResult.TotalPages,
			PageNumber = entityResult.PageNumber,
			PageSize = entityResult.PageSize
		};
	}
}