using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.OrderControllers;
[Authorize(Roles = Roles.CompanyAdmin)]
public class OrderController : BaseV1Controller
{
	private readonly IPlaceOrderService _orderService;
	public OrderController(IPlaceOrderService orderService)
	{
		_orderService = orderService;
	}
	[HttpPost]
	public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto placeOrderDto)
	{
		var result = await _orderService.PlaceOrderAsync(placeOrderDto);
		return CustomResponse(result);
	}
}
