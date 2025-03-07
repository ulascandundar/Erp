using AutoMapper;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Application.Services.OrderServices;

public class PlaceOrderService : IPlaceOrderService
{
    private readonly ErpDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocalizationService _localizationService;

    public PlaceOrderService(
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

    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderDto placeOrderDto)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        if (!currentUser.CompanyId.HasValue)
        {
            throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany));
        }

        // Only validate product prices and payment amounts if IsSafeOrder is true
        if (placeOrderDto.IsSafeOrder)
        {
            await ValidateProductPricesAndPaymentsAsync(placeOrderDto, currentUser.CompanyId.Value);
        }

        // Map PlaceOrderDto to Order entity
        var order = _mapper.Map<Order>(placeOrderDto);

        // Set company and user information
        order.AssignToCompanyAndUser(currentUser.CompanyId.Value, currentUser.Id);
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync();

        // Map Order entity to OrderDto
        var orderDto = _mapper.Map<OrderDto>(order);

        return orderDto;

    }

    /// <summary>
    /// Validates that product prices in the order match the actual product prices in the database
    /// and that the total payment amount is sufficient to cover the order total
    /// </summary>
    /// <param name="placeOrderDto">The order data to validate</param>
    /// <param name="companyId">The company ID for product validation</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    private async Task ValidateProductPricesAndPaymentsAsync(PlaceOrderDto placeOrderDto, Guid companyId)
    {
        // Get all product IDs from order items
        var productIds = placeOrderDto.OrderItems.Select(item => item.ProductId).ToList();

        // Fetch products from database
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.CompanyId == companyId && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, p => p);

        // Validate each order item
        decimal expectedTotalAmount = 0;
        foreach (var orderItem in placeOrderDto.OrderItems)
        {
            // Check if product exists
            if (!products.TryGetValue(orderItem.ProductId, out var product))
            {
                throw new BadRequestException(_localizationService.GetLocalizedString(ResourceKeys.Errors.ProductNotFound));
            }

            // Calculate expected price based on product price and quantity
            decimal expectedPrice = product.Price * orderItem.Quantity;
            
            // Check if the price matches (with a small tolerance for floating point errors)
            if (Math.Abs(expectedPrice - orderItem.TotalAmount) > 0.01m)
            {
                throw new BadRequestException(string.Format(
                    "Ürün fiyatı uyuşmazlığı: {0}. Beklenen: {1}, Gönderilen: {2}",
                    product.Name, expectedPrice, orderItem.TotalAmount));
            }

            // Add to expected total
            expectedTotalAmount += orderItem.TotalAmount;
        }

        // Calculate total payments (excluding discounts which are already applied to order items)
        decimal totalPayments = placeOrderDto.Payments
            .Sum(p => p.Amount);

        // Check if payments are sufficient
        if (totalPayments < expectedTotalAmount)
        {
            throw new BadRequestException(string.Format(
                "Yetersiz ödeme tutarı. Beklenen: {0}, Gönderilen: {1}",
                expectedTotalAmount, totalPayments));
        }
    }
}