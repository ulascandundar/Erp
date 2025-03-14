using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.RawMaterialServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;
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

namespace Erp.Application.Tests.Services.RawMaterialServices
{
    public class RawMaterialServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Mock<IUnitService> _unitServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public RawMaterialServiceTests()
        {
            // InMemory database setup
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper configuration
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RawMaterialProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // HttpContextAccessor mock
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // CurrentUserService mock
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            var currentUser = new UserDto
            {
                Id = _userId,
                CompanyId = _companyId,
                Roles = new List<string> { "CompanyAdmin" }
            };
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(currentUser);
            
            // LocalizationService mock
            _localizationServiceMock = new Mock<ILocalizationService>();
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(key => key);
            _localizationServiceMock.Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns<string, object[]>((key, args) => string.Format(key, args));
                
            // UnitService mock
            _unitServiceMock = new Mock<IUnitService>();
            _unitServiceMock.Setup(x => x.ConvertUnit(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(1.0m); // Default conversion rate is 1
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }
        
        private async Task<Guid> SeedUnitAsync(ErpDbContext context)
        {
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Kilogram",
                ShortCode = "KG",
                Description = "Weight unit",
                ConversionRate = 1,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            await context.Units.AddAsync(unit);
            await context.SaveChangesAsync();
            
            return unit.Id;
        }
        
        private async Task<List<Guid>> SeedSuppliersAsync(ErpDbContext context, int count = 2)
        {
            var supplierIds = new List<Guid>();
            
            for (int i = 1; i <= count; i++)
            {
                var supplier = new Supplier
                {
                    Id = Guid.NewGuid(),
                    Name = $"Supplier {i}",
                    TaxNumber = $"TAX{i}",
                    Address = $"Address {i}",
                    PhoneNumber = $"Phone {i}",
                    Email = $"supplier{i}@example.com",
                    CompanyId = _companyId
                };
                
                await context.Suppliers.AddAsync(supplier);
                supplierIds.Add(supplier.Id);
            }
            
            await context.SaveChangesAsync();
            return supplierIds;
        }

        [Fact]
        public async Task CreateRawMaterialAsync_ValidRawMaterial_ReturnsRawMaterialDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            var supplierIds = await SeedSuppliersAsync(context);
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialCreateDto = new RawMaterialCreateDto
            {
                Name = "Test Raw Material",
                Description = "Test Description",
                Price = 10.5m,
                Barcode = "TEST123",
                Stock = 100,
                UnitId = unitId,
                SupplierIds = supplierIds
            };
            
            // Act
            var result = await rawMaterialService.CreateRawMaterialAsync(rawMaterialCreateDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(rawMaterialCreateDto.Name, result.Name);
            Assert.Equal(rawMaterialCreateDto.Description, result.Description);
            Assert.Equal(rawMaterialCreateDto.Price, result.Price);
            Assert.Equal(rawMaterialCreateDto.Barcode, result.Barcode);
            Assert.Equal(rawMaterialCreateDto.Stock, result.Stock);
            Assert.Equal(rawMaterialCreateDto.UnitId, result.UnitId);
            Assert.Equal(_companyId, result.CompanyId);
            
            // Verify supplier associations were created
            var supplierAssociations = await context.RawMaterialSuppliers
                .Where(rms => rms.RawMaterialId == result.Id && !rms.IsDeleted)
                .ToListAsync();
                
            Assert.Equal(supplierIds.Count, supplierAssociations.Count);
            foreach (var supplierId in supplierIds)
            {
                Assert.Contains(supplierAssociations, sa => sa.SupplierId == supplierId);
            }
        }
        
        [Fact]
        public async Task CreateRawMaterialAsync_NoCompanyId_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(new UserDto
            {
                Id = _userId,
                CompanyId = null // User without company
            });
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialCreateDto = new RawMaterialCreateDto
            {
                Name = "Test Raw Material",
                Description = "Test Description",
                Price = 10.5m,
                Barcode = "TEST123",
                Stock = 100,
                UnitId = unitId
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                rawMaterialService.CreateRawMaterialAsync(rawMaterialCreateDto));
        }
        
        [Fact]
        public async Task CreateRawMaterialAsync_DuplicateName_ThrowsRawMaterialNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialCreateDto = new RawMaterialCreateDto
            {
                Name = "Existing Raw Material", // Same name
                Description = "Test Description",
                Price = 10.5m,
                Barcode = "TEST123", // Different barcode
                Stock = 100,
                UnitId = unitId
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<RawMaterialNameAlreadyExistsException>(() => 
                rawMaterialService.CreateRawMaterialAsync(rawMaterialCreateDto));
        }
        
        [Fact]
        public async Task CreateRawMaterialAsync_DuplicateBarcode_ThrowsRawMaterialBarcodeAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialCreateDto = new RawMaterialCreateDto
            {
                Name = "New Raw Material", // Different name
                Description = "Test Description",
                Price = 10.5m,
                Barcode = "EXISTING123", // Same barcode
                Stock = 100,
                UnitId = unitId
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<RawMaterialBarcodeAlreadyExistsException>(() => 
                rawMaterialService.CreateRawMaterialAsync(rawMaterialCreateDto));
        }
        
        [Fact]
        public async Task UpdateRawMaterialAsync_ValidRawMaterial_ReturnsUpdatedRawMaterialDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            var supplierIds = await SeedSuppliersAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialUpdateDto = new RawMaterialUpdateDto
            {
                Name = "Updated Raw Material",
                Description = "Updated Description",
                Price = 20.0m,
                Barcode = "UPDATED123",
                UnitId = unitId,
                SupplierIds = supplierIds
            };
            
            // Act
            var result = await rawMaterialService.UpdateRawMaterialAsync(existingRawMaterial.Id, rawMaterialUpdateDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(rawMaterialUpdateDto.Name, result.Name);
            Assert.Equal(rawMaterialUpdateDto.Description, result.Description);
            Assert.Equal(rawMaterialUpdateDto.Price, result.Price);
            Assert.Equal(rawMaterialUpdateDto.Barcode, result.Barcode);
            Assert.Equal(rawMaterialUpdateDto.UnitId, result.UnitId);
            
            // Verify supplier associations were created
            var supplierAssociations = await context.RawMaterialSuppliers
                .Where(rms => rms.RawMaterialId == result.Id && !rms.IsDeleted)
                .ToListAsync();
                
            Assert.Equal(supplierIds.Count, supplierAssociations.Count);
            foreach (var supplierId in supplierIds)
            {
                Assert.Contains(supplierAssociations, sa => sa.SupplierId == supplierId);
            }
        }
        
        [Fact]
        public async Task UpdateRawMaterialAsync_RawMaterialNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var rawMaterialUpdateDto = new RawMaterialUpdateDto
            {
                Name = "Updated Raw Material",
                Description = "Updated Description",
                Price = 20.0m,
                Barcode = "UPDATED123",
                UnitId = unitId
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                rawMaterialService.UpdateRawMaterialAsync(Guid.NewGuid(), rawMaterialUpdateDto));
        }
        
        [Fact]
        public async Task GetRawMaterialAsync_ExistingRawMaterial_ReturnsRawMaterialDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            // Act
            var result = await rawMaterialService.GetRawMaterialAsync(existingRawMaterial.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingRawMaterial.Id, result.Id);
            Assert.Equal(existingRawMaterial.Name, result.Name);
            Assert.Equal(existingRawMaterial.Description, result.Description);
            Assert.Equal(existingRawMaterial.Price, result.Price);
            Assert.Equal(existingRawMaterial.Barcode, result.Barcode);
            Assert.Equal(existingRawMaterial.Stock, result.Stock);
            Assert.Equal(existingRawMaterial.UnitId, result.UnitId);
            Assert.Equal(existingRawMaterial.CompanyId, result.CompanyId);
        }
        
        [Fact]
        public async Task GetRawMaterialAsync_RawMaterialNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                rawMaterialService.GetRawMaterialAsync(Guid.NewGuid()));
        }
        
        [Fact]
        public async Task GetRawMaterialsAsync_ReturnsPagedResult()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add multiple raw materials
            for (int i = 1; i <= 10; i++)
            {
                var rawMaterial = new RawMaterial
                {
                    Id = Guid.NewGuid(),
                    Name = $"Raw Material {i}",
                    Description = $"Description {i}",
                    Price = 10.0m + i,
                    Barcode = $"BARCODE{i}",
                    Stock = 50 + i,
                    UnitId = unitId,
                    CompanyId = _companyId
                };
                
                await context.RawMaterials.AddAsync(rawMaterial);
            }
            
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 5,
                Search = "",
                OrderBy = "Name",
                IsDesc = false
            };
            
            // Act
            var result = await rawMaterialService.GetRawMaterialsAsync(paginationRequest);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(5, result.PageSize);
            
            // Verify sorting
            for (int i = 0; i < result.Items.Count - 1; i++)
            {
                Assert.True(string.Compare(result.Items[i].Name, result.Items[i + 1].Name) <= 0);
            }
        }
        
        [Fact]
        public async Task GetRawMaterialsAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add raw materials with different names
            var rawMaterial1 = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Apple",
                Description = "Description 1",
                Price = 10.0m,
                Barcode = "APPLE123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            var rawMaterial2 = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Banana",
                Description = "Description 2",
                Price = 15.0m,
                Barcode = "BANANA123",
                Stock = 60,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            var rawMaterial3 = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Cherry",
                Description = "Description 3",
                Price = 20.0m,
                Barcode = "CHERRY123",
                Stock = 70,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddRangeAsync(rawMaterial1, rawMaterial2, rawMaterial3);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "ban", // Should match "Banana"
                OrderBy = "Name",
                IsDesc = false
            };
            
            // Act
            var result = await rawMaterialService.GetRawMaterialsAsync(paginationRequest);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Banana", result.Items[0].Name);
        }
        
        [Fact]
        public async Task DeleteRawMaterialAsync_ExistingRawMaterial_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            // Act
            await rawMaterialService.DeleteRawMaterialAsync(existingRawMaterial.Id);
            
            // Assert
            var deletedRawMaterial = await context.RawMaterials.FindAsync(existingRawMaterial.Id);
            Assert.NotNull(deletedRawMaterial);
            Assert.True(deletedRawMaterial.IsDeleted);
        }
        
        [Fact]
        public async Task DeleteRawMaterialAsync_RawMaterialNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                rawMaterialService.DeleteRawMaterialAsync(Guid.NewGuid()));
        }
        
        [Fact]
        public async Task DeleteRawMaterialAsync_RawMaterialExistsInFormula_ThrowsRawMaterialExistInFormulaException()
        {
            // Arrange
            using var context = CreateDbContext();
            var unitId = await SeedUnitAsync(context);
            
            // Add existing raw material
            var existingRawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Existing Raw Material",
                Description = "Existing Description",
                Price = 15.0m,
                Barcode = "EXISTING123",
                Stock = 50,
                UnitId = unitId,
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(existingRawMaterial);
            
            // Create a product formula that uses the raw material
            var productFormula = new ProductFormula
            {
                Id = Guid.NewGuid(),
                Name = "Test Formula",
                Description = "Test Formula Description",
                CompanyId = _companyId
            };
            
            await context.ProductFormulas.AddAsync(productFormula);
            
            var productFormulaItem = new ProductFormulaItem
            {
                Id = Guid.NewGuid(),
                ProductFormulaId = productFormula.Id,
                RawMaterialId = existingRawMaterial.Id,
                Quantity = 10
            };
            
            await context.ProductFormulaItems.AddAsync(productFormulaItem);
            await context.SaveChangesAsync();
            
            var rawMaterialService = new RawMaterialService(
                context, 
                _localizationServiceMock.Object, 
                _currentUserServiceMock.Object, 
                _mapper, 
                _unitServiceMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<RawMaterialExistInFormulaException>(() => 
                rawMaterialService.DeleteRawMaterialAsync(existingRawMaterial.Id));
        }
    }
} 