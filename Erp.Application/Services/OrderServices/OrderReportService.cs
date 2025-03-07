using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Erp.Application.Common.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Order;
using Erp.Domain.DTOs.Order.Report;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Erp.Application.Services.OrderServices;

public class OrderReportService : IOrderReportService
{
	private readonly ErpDbContext _db;
	private readonly IMapper _mapper;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILocalizationService _localizationService;
	public OrderReportService(ErpDbContext db, IMapper mapper, ICurrentUserService currentUserService, ILocalizationService localizationService)
	{
		_db = db;
		_mapper = mapper;
		_currentUserService = currentUserService;
		_localizationService = localizationService;
	}
	public async Task<CumulativeOrderReport> GetCumulativeOrderReportAsync(CumulativeOrderReportRequestDto request)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var query = _db.OrderPayment.AsQueryable();
		query = query.Where(x => x.CreatedAt.Date >= request.StartDate.Date && !x.IsDeleted &&
		 x.CreatedAt.Date <= request.EndDate.Date && x.Order.CompanyId == currentUser.CompanyId && !x.Order.IsDeleted);
		var groupQuery = query.GroupBy(x => x.PaymentMethod);
		var report = new CumulativeOrderReport();
		var paymentMethodGroups = await groupQuery.Select(x => new CumulativeOrderPaymentMethodGroup()
		{
			PaymentMethod = x.Key,
			TotalAmount = x.Sum(y => y.Amount),
			TotalDiscount = x.Where(o => o.PaymentMethod == PaymentMethods.Discount).Sum(y => y.Amount),
			TotalNetAmount = 0,
			TotalOrderCount = x.Select(y => y.OrderId).Distinct().Count()
		}).ToListAsync();
		foreach (var item in paymentMethodGroups)
		{
			item.TotalNetAmount = item.TotalAmount - item.TotalDiscount;
		}
		report.PaymentMethodGroups = paymentMethodGroups;
		report.TotalAmount = paymentMethodGroups.Sum(x => x.TotalAmount);
		report.TotalDiscount = paymentMethodGroups.Sum(x => x.TotalDiscount);
		report.TotalNetAmount = paymentMethodGroups.Sum(x => x.TotalNetAmount);
		report.TotalOrderCount = paymentMethodGroups.Sum(x => x.TotalOrderCount);
		return report;
	}

	public async Task<CustomPagedResult<OrderHistoryDto>> GetPagedAsync(PaginationRequest request)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var query = _db.Orders
			.Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
			.AsQueryable();
		if (request.StartDate.HasValue)
		{
			query = query.Where(x => x.CreatedAt.Date >= request.StartDate.Value.Date);
		}
		if (request.EndDate.HasValue)
		{
			query = query.Where(x => x.CreatedAt.Date <= request.EndDate.Value.Date);
		}
		if (string.IsNullOrEmpty(request.OrderBy))
		{
			request.OrderBy = "CreatedAt";
			request.IsDesc = true;
		}

		var entityResult = await query.ToPagedResultAsync(request);
		var dtos = _mapper.Map<List<OrderHistoryDto>>(entityResult.Items);
		return new CustomPagedResult<OrderHistoryDto>
		{
			Items = dtos,
			TotalCount = entityResult.TotalCount,
			TotalPages = entityResult.TotalPages,
			PageNumber = entityResult.PageNumber,
			PageSize = entityResult.PageSize
		};
	}

	public async Task<List<OrderHistoryExcelDto>> GetAllOrdersForExcel(PaginationRequest request)
	{
		var currentUser = _currentUserService.GetCurrentUser();

		if (!currentUser.CompanyId.HasValue)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
		}

		var query = _db.Orders
			.Where(x => x.CompanyId == currentUser.CompanyId.Value && !x.IsDeleted)
			.AsQueryable();
		if (request.StartDate.HasValue)
		{
			query = query.Where(x => x.CreatedAt.Date >= request.StartDate.Value.Date);
		}
		if (request.EndDate.HasValue)
		{
			query = query.Where(x => x.CreatedAt.Date <= request.EndDate.Value.Date);
		}

		var entityResult = await query.ToListAsync();
		entityResult = entityResult.OrderByDescending(x => x.CreatedAt).ToList();
		var dtos = _mapper.Map<List<OrderHistoryExcelDto>>(entityResult);
		return dtos;
	}

	public async Task<CumulativeOrderProductGroupDto> GetCumulativeOrderWithProductGroupReportAsync(CumulativeOrderReportRequestDto request)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var query = _db.OrderItem.AsQueryable();
		query = query.Where(x => x.CreatedAt.Date >= request.StartDate.Date && !x.IsDeleted &&
		 x.CreatedAt.Date <= request.EndDate.Date && x.Order.CompanyId == currentUser.CompanyId && !x.Order.IsDeleted);
		var groupQuery = query.GroupBy(x => new { x.ProductId, ProductName = x.Product.Name });
		var report = new CumulativeOrderProductGroupDto();
		var productGroups = await groupQuery.Select(x => new CumulativeOrderProductGroup()
		{
			ProductId = x.Key.ProductId,
			ProductName = x.Key.ProductName,
			TotalAmount = x.Sum(y => y.TotalAmount),
            TotalQuantityCount = x.Sum(y => y.Quantity)
		}).ToListAsync();
		report.ProductGroups = productGroups;
		report.TotalAmount = productGroups.Sum(x => x.TotalAmount);
		report.TotalQuantityCount = productGroups.Sum(x => x.TotalQuantityCount);
		return report;
	}

	public async Task<OrderDto> GetOrderDetail(Guid id)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var order = await _db.Orders
			.Include(x => x.OrderItems)
			.ThenInclude(x => x.Product)
			.Include(x => x.OrderPayments)
			.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == currentUser.CompanyId && !x.IsDeleted);
		if (order == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
		}
		var dto = _mapper.Map<OrderDto>(order);
		return dto;
	}
}