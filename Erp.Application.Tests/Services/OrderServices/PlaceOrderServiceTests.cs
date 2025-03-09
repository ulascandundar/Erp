using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.OrderServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Order;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Erp.Application.Tests.Services.OrderServices
{
    public class PlaceOrderServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Mock<IUnitService> _unitServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _productId1 = Guid.NewGuid();
        private readonly Guid _productId2 = Guid.NewGuid();
        private readonly Guid _formulaId1 = Guid.NewGuid();
        private readonly Guid _formulaId2 = Guid.NewGuid();
        private readonly Guid _rawMaterialId1 = Guid.NewGuid();
        private readonly Guid _rawMaterialId2 = Guid.NewGuid();
        private readonly Guid _unitId1 = Guid.NewGuid();
        private readonly Guid _unitId2 = Guid.NewGuid();

        public PlaceOrderServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrderProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // HttpContextAccessor mock'u
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // CurrentUserService mock'u
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(new UserDto
            {
                Id = _userId,
                CompanyId = _companyId
            });

            // LocalizationService mock'u
            _localizationServiceMock = new Mock<ILocalizationService>();
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>()))
                .Returns((string key) => key);
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns((string key, object[] args) => string.Format(key, args));
                
            // UnitService mock'u
            _unitServiceMock = new Mock<IUnitService>();
            _unitServiceMock.Setup(x => x.ConvertUnit(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(1.0m); // Varsayılan dönüşüm oranı 1.0
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            return context;
        }

        private async Task SeedTestData(ErpDbContext context)
        {
            // Test birimleri ekle
            var units = new List<Unit>
            {
                new Unit
                {
                    Id = _unitId1,
                    Name = "Kilogram",
                    ShortCode = "KG",
                    ConversionRate = 1,
                    UnitType = UnitType.Weight,
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Unit
                {
                    Id = _unitId2,
                    Name = "Gram",
                    ShortCode = "G",
                    ConversionRate = 0.001m,
                    UnitType = UnitType.Weight,
                    CompanyId = _companyId,
                    IsDeleted = false
                }
            };
            
            await context.Units.AddRangeAsync(units);
            
            // Test hammaddeleri ekle
            var rawMaterials = new List<RawMaterial>
            {
                new RawMaterial
                {
                    Id = _rawMaterialId1,
                    Name = "Test Raw Material 1",
                    Barcode = "RM-001",
                    Stock = 100,
                    UnitId = _unitId1,
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new RawMaterial
                {
                    Id = _rawMaterialId2,
                    Name = "Test Raw Material 2",
                    Barcode = "RM-002",
                    Stock = 200,
                    UnitId = _unitId2,
                    CompanyId = _companyId,
                    IsDeleted = false
                }
            };
            
            await context.RawMaterials.AddRangeAsync(rawMaterials);
            
            // Test formülleri ekle
            var formulas = new List<ProductFormula>
            {
                new ProductFormula
                {
                    Id = _formulaId1,
                    Name = "Test Formula 1",
                    CompanyId = _companyId,
                    IsDeleted = false,
                    Items = new List<ProductFormulaItem>
                    {
                        new ProductFormulaItem
                        {
                            RawMaterialId = _rawMaterialId1,
                            Quantity = 2,
                            UnitId = _unitId1
                        }
                    }
                },
                new ProductFormula
                {
                    Id = _formulaId2,
                    Name = "Test Formula 2",
                    CompanyId = _companyId,
                    IsDeleted = false,
                    Items = new List<ProductFormulaItem>
                    {
                        new ProductFormulaItem
                        {
                            RawMaterialId = _rawMaterialId2,
                            Quantity = 50,
                            UnitId = _unitId2
                        }
                    }
                }
            };
            
            await context.ProductFormulas.AddRangeAsync(formulas);

            // Test ürünleri ekle
            var products = new List<Product>
            {
                new Product
                {
                    Id = _productId1,
                    Name = "Test Product 1",
                    SKU = "TST-001",
                    Price = 100,
                    CompanyId = _companyId,
                    ProductFormulaId = _formulaId1,
                    IsDeleted = false
                },
                new Product
                {
                    Id = _productId2,
                    Name = "Test Product 2",
                    SKU = "TST-002",
                    Price = 200,
                    CompanyId = _companyId,
                    ProductFormulaId = _formulaId2,
                    IsDeleted = false
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task PlaceOrderAsync_ValidSafeOrder_ReturnsOrderDto()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // 2 * 100
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // 1 * 200
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 400, // 200 + 200
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order",
                IsSafeOrder = true // Güvenli sipariş
            };

            // Act
            var result = await placeOrderService.PlaceOrderAsync(placeOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(400, result.TotalAmount);
            Assert.Equal(0, result.DiscountAmount);
            Assert.Equal(400, result.NetAmount);
            Assert.Equal(3, result.TotalQuantity); // 2 + 1
            Assert.Equal(2, result.OrderItems.Count);
            Assert.Equal(1, result.OrderPayments.Count);
        }

        [Fact]
        public async Task PlaceOrderAsync_ValidUnsafeOrder_SkipsValidationAndReturnsOrderDto()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 250, // Yetersiz ödeme (doğrusu 400 olmalı)
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Wrong Prices",
                IsSafeOrder = false // Güvensiz sipariş
            };

            // Act
            var result = await placeOrderService.PlaceOrderAsync(placeOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(300, result.TotalAmount); // 150 + 150
            Assert.Equal(0, result.DiscountAmount);
            Assert.Equal(300, result.NetAmount);
            Assert.Equal(3, result.TotalQuantity); // 2 + 1
            Assert.Equal(2, result.OrderItems.Count);
            Assert.Equal(1, result.OrderPayments.Count);
        }

        [Fact]
        public async Task PlaceOrderAsync_InvalidProductPrice_ThrowsBadRequestException()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // Doğru fiyat
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 350, // 150 + 200
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Wrong Price",
                IsSafeOrder = true // Güvenli sipariş
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => placeOrderService.PlaceOrderAsync(placeOrderDto));
            Assert.Contains("Product price mismatch", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderAsync_InsufficientPayment_ThrowsBadRequestException()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // Doğru fiyat
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // Doğru fiyat
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 350, // Yetersiz ödeme (doğrusu 400 olmalı)
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Insufficient Payment",
                IsSafeOrder = true // Güvenli sipariş
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => placeOrderService.PlaceOrderAsync(placeOrderDto));
            Assert.Contains("Total payment amount does not match", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderAsync_NoCompanyId_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            // Şirketi olmayan kullanıcı
            var currentUserWithoutCompany = new UserDto
            {
                Id = _userId,
                CompanyId = null
            };
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(currentUserWithoutCompany);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 1,
                        TotalAmount = 100,
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 100,
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order without Company",
                IsSafeOrder = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullValueException>(() => placeOrderService.PlaceOrderAsync(placeOrderDto));
            Assert.Equal(ResourceKeys.Errors.UserNotBelongToCompany, exception.Message);
        }

        [Fact]
        public async Task PlaceOrderAsync_WithDiscount_CalculatesNetAmountCorrectly()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // 2 * 100
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // 1 * 200
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 320, // 400 - 80 (indirim)
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    },
                    new PlaceOrderPaymentDto
                    {
                        Amount = 80,
                        PaymentMethod = PaymentMethods.Discount,
                        Description = "Discount"
                    }
                },
                Description = "Test Order with Discount",
                IsSafeOrder = true
            };

            // Act
            var result = await placeOrderService.PlaceOrderAsync(placeOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(400, result.TotalAmount);
            Assert.Equal(80, result.DiscountAmount);
            Assert.Equal(320, result.NetAmount); // 400 - 80
            Assert.Equal(3, result.TotalQuantity); // 2 + 1
            Assert.Equal(2, result.OrderItems.Count);
            Assert.Equal(2, result.OrderPayments.Count);
        }

        [Fact]
        public async Task PlaceOrderAsync_WithFormula_ReducesRawMaterialStock()
        {
            // Arrange
            using var context = CreateDbContext();
            await SeedTestData(context);

            // UnitService mock'unu ayarla
            _unitServiceMock.Setup(x => x.ConvertUnit(_unitId1, _rawMaterialId1)).ReturnsAsync(1.0m);
            _unitServiceMock.Setup(x => x.ConvertUnit(_unitId2, _rawMaterialId2)).ReturnsAsync(0.001m);

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object,
                _unitServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // 2 * 100
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 3,
                        TotalAmount = 600, // 3 * 200
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 800, // 200 + 600
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Formula",
                IsSafeOrder = true
            };

            // İlk stok miktarlarını kaydet
            var initialRawMaterial1 = await context.RawMaterials.FindAsync(_rawMaterialId1);
            var initialRawMaterial2 = await context.RawMaterials.FindAsync(_rawMaterialId2);
            var initialStock1 = initialRawMaterial1.Stock;
            var initialStock2 = initialRawMaterial2.Stock;

            // Act
            var result = await placeOrderService.PlaceOrderAsync(placeOrderDto);

            // Assert
            // Sipariş başarıyla oluşturuldu mu kontrol et
            Assert.NotNull(result);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(800, result.TotalAmount);

            // Hammadde stokları azaldı mı kontrol et
            var updatedRawMaterial1 = await context.RawMaterials.FindAsync(_rawMaterialId1);
            var updatedRawMaterial2 = await context.RawMaterials.FindAsync(_rawMaterialId2);
            
            // Ürün 1: 2 adet sipariş edildi, her biri 2 birim hammadde kullanıyor, toplam 4 birim azalmalı
            // Dönüşüm oranı 1.0 olduğu için 4 birim azalır
            Assert.Equal(initialStock1 - 4, updatedRawMaterial1.Stock);
            
            // Ürün 2: 3 adet sipariş edildi, her biri 50 birim hammadde kullanıyor, toplam 150 birim azalmalı
            // Dönüşüm oranı 0.001 olduğu için 150 * 0.001 = 0.15 birim azalır
            Assert.Equal(initialStock2 - 150, updatedRawMaterial2.Stock);
        }
    }
} 