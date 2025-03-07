using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Order.Report;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.OrderControllers;
[Authorize(Roles = Roles.CompanyAdmin)]
public class OrderReportController : BaseV1Controller
{
	private readonly IOrderReportService _orderReportService;
	public OrderReportController(IOrderReportService orderReportService)
	{
		_orderReportService = orderReportService;
	}
	[HttpGet]
	public async Task<IActionResult> GetCumulativeOrderReport([FromQuery] CumulativeOrderReportRequestDto request)
	{
		var report = await _orderReportService.GetCumulativeOrderReportAsync(request);
		return CustomResponse(report);
	}

	[HttpGet("history")]
	public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest request)
	{
		var report = await _orderReportService.GetPagedAsync(request);
		return CustomResponse(report);
	}

	[HttpGet("history-excel")]
	public async Task<IActionResult> HistoryExcel([FromQuery] PaginationRequest request)
	{
		var result = await _orderReportService.GetAllOrdersForExcel(request);
		return ExportToExcel(result, "OrderHistory");
	}

	[HttpGet("product-group")]
	public async Task<IActionResult> GetCumulativeOrderWithProductGroupReport([FromQuery] CumulativeOrderReportRequestDto request)
	{
		var report = await _orderReportService.GetCumulativeOrderWithProductGroupReportAsync(request);
		return CustomResponse(report);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetDetail(Guid id)
	{
		var result = await _orderReportService.GetOrderDetail(id);
		return CustomResponse(result);
	}
}
