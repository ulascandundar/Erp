using Erp.Domain.DTOs.Order;
using System;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IPlaceOrderService
{
    Task<OrderDto> PlaceOrderAsync(PlaceOrderDto placeOrderDto);
} 