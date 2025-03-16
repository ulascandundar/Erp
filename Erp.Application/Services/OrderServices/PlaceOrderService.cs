using AutoMapper;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;
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
    private readonly IUnitService _unitService;

    public PlaceOrderService(
        ErpDbContext db,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILocalizationService localizationService,
        IUnitService unitService)
    {
        _db = db;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizationService = localizationService;
        _unitService = unitService;
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
        await ProccessStockAsync(placeOrderDto, order, currentUser.CompanyId.Value);
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

    private async Task ProccessStockAsync(PlaceOrderDto placeOrderDto, Order order, Guid companyId)
    {
        var productIds = placeOrderDto.OrderItems.Select(item => item.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.CompanyId == companyId && !p.IsDeleted)
            .Include(p => p.ProductFormula.Items).ThenInclude(p => p.Unit)
            .Include(p => p.ProductFormula.Items).ThenInclude(p => p.RawMaterial).ThenInclude(p => p.Unit)
            .ToListAsync();

        // Ürün ID'lerine göre ürünleri eşleştir
        var productDict = products.ToDictionary(p => p.Id, p => p);

        foreach (var orderItem in order.OrderItems)
        {
            if (productDict.TryGetValue(orderItem.ProductId, out var product) && product.ProductFormula != null)
            {
                // Ürün maliyetini hesapla ve hammadde tüketim raporunu oluştur
                var costResult = await CalculateProductCostAndConsumptionReportAsync(product, orderItem.Quantity);
                decimal productCost = costResult.ProductCost;
                List<RawMaterialConsumption> consumptionReport = costResult.ConsumptionReport;
                
                // OrderItem'a maliyet ve tüketim raporu bilgilerini ekle
                orderItem.ProductCost = productCost;
                orderItem.RawMaterialConsumptionReport = consumptionReport;
                
                // Hammadde stoklarını güncelle
                foreach (var formulaItem in product.ProductFormula.Items)
                {
                    var rate = await _unitService.ConvertUnit(formulaItem.UnitId, formulaItem.RawMaterialId);
                    var consumedQuantity = (formulaItem.Quantity * orderItem.Quantity) / rate;
                    formulaItem.RawMaterial.Stock -= consumedQuantity;
                    _db.RawMaterials.Update(formulaItem.RawMaterial);
                }
            }
        }
    }

    /// <summary>
    /// Ürünün maliyetini hesaplar ve hammadde tüketim raporunu oluşturur
    /// </summary>
    /// <param name="product">Ürün</param>
    /// <param name="quantity">Sipariş edilen miktar</param>
    /// <returns>Ürün maliyeti ve hammadde tüketim raporu</returns>
    private async Task<(decimal ProductCost, List<RawMaterialConsumption> ConsumptionReport)> CalculateProductCostAndConsumptionReportAsync(Product product, decimal quantity)
    {
        decimal totalCost = 0;
        var consumptionReport = new List<RawMaterialConsumption>();

        if (product.ProductFormula == null || !product.ProductFormula.Items.Any())
        {
            return (0, consumptionReport);
        }

        foreach (var formulaItem in product.ProductFormula.Items)
        {
            var rawMaterial = formulaItem.RawMaterial;
            if (rawMaterial == null) continue;

            // Birim dönüşümü yap
            var rate = await _unitService.ConvertUnit(formulaItem.UnitId, formulaItem.RawMaterialId);
            var consumedQuantity = (formulaItem.Quantity * quantity) / rate;
            
            // Hammadde maliyetini hesapla
            var itemCost = rawMaterial.Price * consumedQuantity;
            totalCost += itemCost;

            // Tüketim raporuna ekle
            consumptionReport.Add(new RawMaterialConsumption
            {
                RawMaterialId = rawMaterial.Id,
                RawMaterialName = rawMaterial.Name,
                UnitId = rawMaterial.UnitId,
                UnitName = rawMaterial.Unit?.Name,
                ConsumedQuantity = consumedQuantity,
                UnitPrice = rawMaterial.Price,
                TotalCost = itemCost
            });
        }

        return (totalCost, consumptionReport);
    }
}