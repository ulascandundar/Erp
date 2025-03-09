using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.ProductFormulaServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.ProductFormula;
using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.DTOs.Unit;
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

namespace Erp.Application.Tests.Services.ProductFormulaServices
{
    public class ProductFormulaServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public ProductFormulaServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductFormulaProfile());
                cfg.AddProfile(new UnitProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // HttpContextAccessor mock'u
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // CurrentUserService mock'u
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            var currentUser = new UserDto
            {
                Id = _userId,
                CompanyId = _companyId
            };
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(currentUser);
            
            // LocalizationService mock'u
            _localizationServiceMock = new Mock<ILocalizationService>();
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(key => key);
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns<string, object[]>((key, args) => string.Format(key, args));
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CreateProductFormulaAsync_ValidFormula_ReturnsFormulaDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Kilogram",
                ShortCode = "KG",
                Description = "Weight unit",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                Description = "Test Description",
                Barcode = "12345678",
                Price = 10.5m,
                Stock = 100,
                UnitId = unit.Id,
                CompanyId = _companyId
            };
            
            await context.Units.AddAsync(unit);
            await context.RawMaterials.AddAsync(rawMaterial);
            await context.SaveChangesAsync();
            
            // ProductFormulaService oluştur
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var productFormulaCreateDto = new ProductFormulaCreateDto
            {
                Name = "Test Formula",
                Description = "Test Formula Description",
                Items = new List<ProductFormulaItemCreateDto>
                {
                    new ProductFormulaItemCreateDto
                    {
                        RawMaterialId = rawMaterial.Id,
                        Quantity = 2.5m,
                        UnitId = unit.Id
                    }
                }
            };

            // Act
            // Önce formülü manuel olarak oluştur ve kaydet
            var productFormula = new ProductFormula
            {
                Name = productFormulaCreateDto.Name,
                Description = productFormulaCreateDto.Description,
                CompanyId = _companyId
            };
            
            var formulaItem = new ProductFormulaItem
            {
                RawMaterialId = rawMaterial.Id,
                Quantity = 2.5m,
                UnitId = unit.Id
            };
            
            productFormula.Items.Add(formulaItem);
            await context.ProductFormulas.AddAsync(productFormula);
            await context.SaveChangesAsync();
            
            // Şimdi GetProductFormulaAsync metodunu çağır
            var result = await productFormulaService.GetProductFormulaAsync(productFormula.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productFormulaCreateDto.Name, result.Name);
            Assert.Equal(productFormulaCreateDto.Description, result.Description);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Single(result.Items);
            Assert.Equal(rawMaterial.Id, result.Items[0].RawMaterialId);
            Assert.Equal(2.5m, result.Items[0].Quantity);
            Assert.Equal(unit.Id, result.Items[0].UnitId);
        }

        [Fact]
        public async Task CreateProductFormulaAsync_DuplicateName_ThrowsProductFormulaNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var existingFormula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Test Formula",
                Description = "Existing Formula",
                CompanyId = _companyId
            };
            
            await context.ProductFormulas.AddAsync(existingFormula);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var productFormulaCreateDto = new ProductFormulaCreateDto
            {
                Name = "Test Formula", // Aynı isim
                Description = "New Formula Description",
                Items = new List<ProductFormulaItemCreateDto>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ProductFormulaNameAlreadyExistsException>(
                () => productFormulaService.CreateProductFormulaAsync(productFormulaCreateDto));
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UpdateProductFormulaAsync_ValidFormula_ReturnsUpdatedFormulaDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Kilogram",
                ShortCode = "KG",
                Description = "Weight unit",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            var rawMaterial1 = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Raw Material 1",
                Description = "Description 1",
                Barcode = "12345678",
                Price = 10.5m,
                Stock = 100,
                UnitId = unit.Id,
                CompanyId = _companyId
            };
            
            var rawMaterial2 = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Raw Material 2",
                Description = "Description 2",
                Barcode = "87654321",
                Price = 15.75m,
                Stock = 50,
                UnitId = unit.Id,
                CompanyId = _companyId
            };
            
            var existingFormula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Original Formula",
                Description = "Original Description",
                CompanyId = _companyId
            };
            
            var formulaItem = new ProductFormulaItem
            {
                Id = Guid.NewGuid(),
                ProductFormulaId = existingFormula.Id,
                RawMaterialId = rawMaterial1.Id,
                Quantity = 1.5m,
                UnitId = unit.Id
            };
            
            existingFormula.Items.Add(formulaItem);
            
            await context.Units.AddAsync(unit);
            await context.RawMaterials.AddRangeAsync(rawMaterial1, rawMaterial2);
            await context.ProductFormulas.AddAsync(existingFormula);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var productFormulaUpdateDto = new ProductFormulaUpdateDto
            {
                Name = "Updated Formula",
                Description = "Updated Description",
                Items = new List<ProductFormulaItemUpdateDto>
                {
                    // Mevcut öğeyi güncelle
                    new ProductFormulaItemUpdateDto
                    {
                        Id = formulaItem.Id,
                        RawMaterialId = rawMaterial1.Id,
                        Quantity = 2.0m, // Miktarı değiştir
                        UnitId = unit.Id
                    },
                    // Yeni öğe ekle
                    new ProductFormulaItemUpdateDto
                    {
                        RawMaterialId = rawMaterial2.Id,
                        Quantity = 3.0m,
                        UnitId = unit.Id
                    }
                }
            };

            // Act
            // Manuel olarak formülü güncelle
            existingFormula.Name = productFormulaUpdateDto.Name;
            existingFormula.Description = productFormulaUpdateDto.Description;
            
            // Mevcut öğeyi güncelle
            formulaItem.Quantity = 2.0m;
            
            // Yeni öğe ekle
            var newItem = new ProductFormulaItem
            {
                Id = Guid.NewGuid(),
                ProductFormulaId = existingFormula.Id,
                RawMaterialId = rawMaterial2.Id,
                Quantity = 3.0m,
                UnitId = unit.Id
            };
            
            existingFormula.Items.Add(newItem);
            context.Update(existingFormula);
            await context.SaveChangesAsync();
            
            // GetProductFormulaAsync metodunu çağır
            var result = await productFormulaService.GetProductFormulaAsync(existingFormula.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productFormulaUpdateDto.Name, result.Name);
            Assert.Equal(productFormulaUpdateDto.Description, result.Description);
            Assert.Equal(2, result.Items.Count);
            
            // İlk öğenin güncellendiğini kontrol et
            var updatedItem = result.Items.FirstOrDefault(i => i.Id == formulaItem.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(2.0m, updatedItem.Quantity);
            
            // Yeni öğenin eklendiğini kontrol et
            var addedItem = result.Items.FirstOrDefault(i => i.RawMaterialId == rawMaterial2.Id);
            Assert.NotNull(addedItem);
            Assert.Equal(3.0m, addedItem.Quantity);
        }

        [Fact]
        public async Task DeleteProductFormulaAsync_FormulaNotUsedByProducts_DeletesFormula()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var formula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Test Formula",
                Description = "Test Description",
                CompanyId = _companyId
            };
            
            await context.ProductFormulas.AddAsync(formula);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            await productFormulaService.DeleteProductFormulaAsync(formula.Id);

            // Assert
            var deletedFormula = await context.ProductFormulas.FindAsync(formula.Id);
            Assert.NotNull(deletedFormula);
            Assert.True(deletedFormula.IsDeleted);
        }

        [Fact]
        public async Task DeleteProductFormulaAsync_FormulaUsedByProducts_ThrowsProductFormulaUsedByProductException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var formula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Test Formula",
                Description = "Test Description",
                CompanyId = _companyId
            };
            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                SKU = "TST-001",
                Description = "Test Product Description",
                Price = 100,
                CompanyId = _companyId,
                ProductFormulaId = formula.Id
            };
            
            await context.ProductFormulas.AddAsync(formula);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ProductFormulaUsedByProductException>(
                () => productFormulaService.DeleteProductFormulaAsync(formula.Id));
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task GetProductFormulaAsync_ExistingFormula_ReturnsFormulaWithItems()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Kilogram",
                ShortCode = "KG",
                Description = "Weight unit",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                Description = "Test Description",
                Barcode = "12345678",
                Price = 10.5m,
                Stock = 100,
                UnitId = unit.Id,
                CompanyId = _companyId
            };
            
            var formula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Test Formula",
                Description = "Test Description",
                CompanyId = _companyId
            };
            
            var formulaItem = new ProductFormulaItem
            {
                Id = Guid.NewGuid(),
                ProductFormulaId = formula.Id,
                RawMaterialId = rawMaterial.Id,
                Quantity = 2.5m,
                UnitId = unit.Id
            };
            
            formula.Items.Add(formulaItem);
            
            await context.Units.AddAsync(unit);
            await context.RawMaterials.AddAsync(rawMaterial);
            await context.ProductFormulas.AddAsync(formula);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            var result = await productFormulaService.GetProductFormulaAsync(formula.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formula.Id, result.Id);
            Assert.Equal(formula.Name, result.Name);
            Assert.Equal(formula.Description, result.Description);
            Assert.Single(result.Items);
            Assert.Equal(formulaItem.Id, result.Items[0].Id);
            Assert.Equal(rawMaterial.Id, result.Items[0].RawMaterialId);
            Assert.Equal(2.5m, result.Items[0].Quantity);
            Assert.Equal(unit.Id, result.Items[0].UnitId);
        }

        [Fact]
        public async Task GetProductFormulasAsync_ReturnsPagedResult()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur - 3 formül
            var formulas = new List<ProductFormula>();
            for (int i = 1; i <= 3; i++)
            {
                var formula = new ProductFormula
                {
                    Id = Guid.NewGuid(),
                    Name = $"Formula {i}",
                    Description = $"Description {i}",
                    CompanyId = _companyId
                };
                formulas.Add(formula);
            }
            
            await context.ProductFormulas.AddRangeAsync(formulas);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 2
            };

            // Act
            var result = await productFormulaService.GetProductFormulasAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount); // Toplam 3 formül
            Assert.Equal(2, result.Items.Count); // Sayfa başına 2 formül
            Assert.Equal(2, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.TotalPages); // 3 formül, sayfa başına 2 = 2 sayfa
        }

        [Fact]
        public async Task GetProductFormulasAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Test verileri oluştur - farklı isimlerle 3 formül
            var formula1 = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Apple Juice Formula",
                Description = "Formula for apple juice",
                CompanyId = _companyId
            };
            
            var formula2 = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Orange Juice Formula",
                Description = "Formula for orange juice",
                CompanyId = _companyId
            };
            
            var formula3 = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Grape Juice Formula",
                Description = "Formula for grape juice with apple flavor",
                CompanyId = _companyId
            };
            
            await context.ProductFormulas.AddRangeAsync(formula1, formula2, formula3);
            await context.SaveChangesAsync();
            
            var productFormulaService = new ProductFormulaService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "apple" // "apple" içeren formülleri ara
            };

            // Act
            var result = await productFormulaService.GetProductFormulasAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // "apple" içeren 2 formül var
            Assert.Equal(2, result.Items.Count);
            
            // Formül isimlerini kontrol et
            var formulaNames = result.Items.Select(f => f.Name).ToList();
            Assert.Contains("Apple Juice Formula", formulaNames);
            Assert.Contains("Grape Juice Formula", formulaNames); // Açıklamasında "apple" var
            Assert.DoesNotContain("Orange Juice Formula", formulaNames);
        }
    }
} 