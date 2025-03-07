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
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _productId1 = Guid.NewGuid();
        private readonly Guid _productId2 = Guid.NewGuid();

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
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            return context;
        }

        private async Task SeedTestData(ErpDbContext context)
        {
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
                    IsDeleted = false
                },
                new Product
                {
                    Id = _productId2,
                    Name = "Test Product 2",
                    SKU = "TST-002",
                    Price = 200,
                    CompanyId = _companyId,
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
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // 2 * 100
                        DiscountAmount = 0
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // 1 * 200
                        DiscountAmount = 0
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
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                        DiscountAmount = 0
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                        DiscountAmount = 0
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
                IsSafeOrder = false // Güvenli olmayan sipariş
            };

            // Act
            var result = await placeOrderService.PlaceOrderAsync(placeOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(250, result.TotalAmount); // Yanlış toplam
            Assert.Equal(0, result.DiscountAmount);
            Assert.Equal(250, result.NetAmount); // Yanlış net tutar
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
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 150, // Yanlış fiyat (doğrusu 200 olmalı)
                        DiscountAmount = 0
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 150,
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Wrong Price",
                IsSafeOrder = true // Güvenli sipariş
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                placeOrderService.PlaceOrderAsync(placeOrderDto));
            
            Assert.Contains("Ürün fiyatı uyuşmazlığı", exception.Message);
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
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 200, // Doğru fiyat
                        DiscountAmount = 0
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 200, // Doğru fiyat
                        DiscountAmount = 0
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 300, // Yetersiz ödeme (doğrusu 400 olmalı)
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    }
                },
                Description = "Test Order with Insufficient Payment",
                IsSafeOrder = true // Güvenli sipariş
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                placeOrderService.PlaceOrderAsync(placeOrderDto));
            
            Assert.Contains("Yetersiz ödeme tutarı", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderAsync_NoCompanyId_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // CompanyId olmayan bir kullanıcı oluştur
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(new UserDto
            {
                Id = _userId,
                CompanyId = null // CompanyId yok
            });

            var placeOrderService = new PlaceOrderService(
                context, 
                _mapper, 
                currentUserServiceMock.Object, 
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 1,
                        TotalAmount = 100,
                        DiscountAmount = 0
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
                Description = "Test Order"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                placeOrderService.PlaceOrderAsync(placeOrderDto));
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
                _localizationServiceMock.Object);

            var placeOrderDto = new PlaceOrderDto
            {
                OrderItems = new List<PlaceOrderItemDto>
                {
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId1,
                        Quantity = 2,
                        TotalAmount = 180, // 200 - 20 indirim
                        DiscountAmount = 20
                    },
                    new PlaceOrderItemDto
                    {
                        ProductId = _productId2,
                        Quantity = 1,
                        TotalAmount = 180, // 200 - 20 indirim
                        DiscountAmount = 20
                    }
                },
                Payments = new List<PlaceOrderPaymentDto>
                {
                    new PlaceOrderPaymentDto
                    {
                        Amount = 320, // 360 - 40 indirim
                        PaymentMethod = PaymentMethods.Cash,
                        Description = "Cash payment"
                    },
                    new PlaceOrderPaymentDto
                    {
                        Amount = 40,
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
            Assert.Equal(360, result.TotalAmount); // 320 + 40
            Assert.Equal(40, result.DiscountAmount);
            Assert.Equal(320, result.NetAmount); // 360 - 40
            Assert.Equal(3, result.TotalQuantity); // 2 + 1
        }
    }
} 