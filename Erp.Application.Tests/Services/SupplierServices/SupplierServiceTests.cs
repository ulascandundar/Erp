using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.SupplierServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Supplier;
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

namespace Erp.Application.Tests.Services.SupplierServices
{
    public class SupplierServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public SupplierServiceTests()
        {
            // InMemory database setup
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper configuration
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new SupplierProfile());
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
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }
        
        private SupplierCreateDto CreateValidSupplierCreateDto()
        {
            return new SupplierCreateDto
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                TaxNumber = "TX123456",
                Website = "https://example.com",
                Description = "Test Description"
            };
        }
        
        private SupplierUpdateDto CreateValidSupplierUpdateDto()
        {
            return new SupplierUpdateDto
            {
                Name = "Updated Supplier",
                ContactPerson = "Jane Smith",
                Email = "updated@example.com",
                PhoneNumber = "0987654321",
                Address = "Updated Address",
                TaxNumber = "TX654321",
                Website = "https://updated-example.com",
                Description = "Updated Description"
            };
        }
        
        private async Task<Guid> SeedSupplierAsync(ErpDbContext context, string name = "Existing Supplier", string taxNumber = "TX999999")
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = name,
                ContactPerson = "Existing Contact",
                Email = "existing@example.com",
                PhoneNumber = "9999999999",
                Address = "Existing Address",
                TaxNumber = taxNumber,
                Website = "https://existing-example.com",
                Description = "Existing Description",
                CompanyId = _companyId
            };
            
            await context.Suppliers.AddAsync(supplier);
            await context.SaveChangesAsync();
            
            return supplier.Id;
        }
        
        private async Task<(Guid SupplierId, Guid RawMaterialId)> SeedSupplierWithRawMaterialAsync(ErpDbContext context)
        {
            // Create supplier
            var supplierId = await SeedSupplierAsync(context);
            
            // Create raw material
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                Description = "Test Description",
                Price = 10.0m,
                Barcode = "TEST123",
                Stock = 100,
                UnitId = Guid.NewGuid(),
                CompanyId = _companyId
            };
            
            await context.RawMaterials.AddAsync(rawMaterial);
            
            // Create association
            var rawMaterialSupplier = new RawMaterialSupplier
            {
                SupplierId = supplierId,
                RawMaterialId = rawMaterial.Id,
                Price = 10.0m
            };
            
            await context.RawMaterialSuppliers.AddAsync(rawMaterialSupplier);
            await context.SaveChangesAsync();
            
            return (supplierId, rawMaterial.Id);
        }

        [Fact]
        public async Task CreateSupplierAsync_ValidSupplier_ReturnsSupplierDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var supplierService = new SupplierService(context, _currentUserServiceMock.Object, _localizationServiceMock.Object, _mapper);
            var supplierCreateDto = CreateValidSupplierCreateDto();
            
            // Act
            var result = await supplierService.CreateSupplierAsync(supplierCreateDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplierCreateDto.Name, result.Name);
            Assert.Equal(supplierCreateDto.ContactPerson, result.ContactPerson);
            Assert.Equal(supplierCreateDto.Email, result.Email);
            Assert.Equal(supplierCreateDto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(supplierCreateDto.Address, result.Address);
            Assert.Equal(supplierCreateDto.TaxNumber, result.TaxNumber);
            Assert.Equal(supplierCreateDto.Website, result.Website);
            Assert.Equal(supplierCreateDto.Description, result.Description);
            Assert.Equal(_companyId, result.CompanyId);
            
            // Verify supplier was added to database
            var savedSupplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == result.Id);
            Assert.NotNull(savedSupplier);
            Assert.Equal(supplierCreateDto.Name, savedSupplier.Name);
        }
        
        [Fact]
        public async Task CreateSupplierAsync_NoCompanyId_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(new UserDto
            {
                Id = _userId,
                CompanyId = null // User without company
            });
            
            var supplierService = new SupplierService(
                context, 
                currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierCreateDto = CreateValidSupplierCreateDto();
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                supplierService.CreateSupplierAsync(supplierCreateDto));
        }
        
        [Fact]
        public async Task CreateSupplierAsync_DuplicateName_ThrowsSupplierNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add existing supplier
            await SeedSupplierAsync(context, "Existing Supplier", "TX999999");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierCreateDto = CreateValidSupplierCreateDto();
            supplierCreateDto.Name = "Existing Supplier"; // Same name
            
            // Act & Assert
            await Assert.ThrowsAsync<SupplierNameAlreadyExistsException>(() => 
                supplierService.CreateSupplierAsync(supplierCreateDto));
        }
        
        [Fact]
        public async Task CreateSupplierAsync_DuplicateTaxNumber_ThrowsSupplierTaxNumberAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add existing supplier
            await SeedSupplierAsync(context, "Existing Supplier", "TX999999");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierCreateDto = CreateValidSupplierCreateDto();
            supplierCreateDto.TaxNumber = "TX999999"; // Same tax number
            
            // Act & Assert
            await Assert.ThrowsAsync<SupplierTaxNumberAlreadyExistsException>(() => 
                supplierService.CreateSupplierAsync(supplierCreateDto));
        }
        
        [Fact]
        public async Task UpdateSupplierAsync_ValidSupplier_ReturnsUpdatedSupplierDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add existing supplier
            var supplierId = await SeedSupplierAsync(context);
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierUpdateDto = CreateValidSupplierUpdateDto();
            
            // Act
            var result = await supplierService.UpdateSupplierAsync(supplierId, supplierUpdateDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplierId, result.Id);
            Assert.Equal(supplierUpdateDto.Name, result.Name);
            Assert.Equal(supplierUpdateDto.ContactPerson, result.ContactPerson);
            Assert.Equal(supplierUpdateDto.Email, result.Email);
            Assert.Equal(supplierUpdateDto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(supplierUpdateDto.Address, result.Address);
            Assert.Equal(supplierUpdateDto.TaxNumber, result.TaxNumber);
            Assert.Equal(supplierUpdateDto.Website, result.Website);
            Assert.Equal(supplierUpdateDto.Description, result.Description);
            
            // Verify supplier was updated in database
            var updatedSupplier = await context.Suppliers.FindAsync(supplierId);
            Assert.NotNull(updatedSupplier);
            Assert.Equal(supplierUpdateDto.Name, updatedSupplier.Name);
        }
        
        [Fact]
        public async Task UpdateSupplierAsync_SupplierNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierUpdateDto = CreateValidSupplierUpdateDto();
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                supplierService.UpdateSupplierAsync(Guid.NewGuid(), supplierUpdateDto));
        }
        
        [Fact]
        public async Task UpdateSupplierAsync_DuplicateName_ThrowsSupplierNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add two suppliers
            var supplierId1 = await SeedSupplierAsync(context, "Supplier 1", "TX111111");
            var supplierId2 = await SeedSupplierAsync(context, "Supplier 2", "TX222222");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierUpdateDto = CreateValidSupplierUpdateDto();
            supplierUpdateDto.Name = "Supplier 2"; // Try to update supplier 1 with supplier 2's name
            
            // Act & Assert
            await Assert.ThrowsAsync<SupplierNameAlreadyExistsException>(() => 
                supplierService.UpdateSupplierAsync(supplierId1, supplierUpdateDto));
        }
        
        [Fact]
        public async Task UpdateSupplierAsync_DuplicateTaxNumber_ThrowsSupplierTaxNumberAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add two suppliers
            var supplierId1 = await SeedSupplierAsync(context, "Supplier 1", "TX111111");
            var supplierId2 = await SeedSupplierAsync(context, "Supplier 2", "TX222222");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var supplierUpdateDto = CreateValidSupplierUpdateDto();
            supplierUpdateDto.TaxNumber = "TX222222"; // Try to update supplier 1 with supplier 2's tax number
            
            // Act & Assert
            await Assert.ThrowsAsync<SupplierTaxNumberAlreadyExistsException>(() => 
                supplierService.UpdateSupplierAsync(supplierId1, supplierUpdateDto));
        }
        
        [Fact]
        public async Task DeleteSupplierAsync_ExistingSupplier_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add existing supplier
            var supplierId = await SeedSupplierAsync(context);
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
            
            // Act
            await supplierService.DeleteSupplierAsync(supplierId);
            
            // Assert
            var deletedSupplier = await context.Suppliers.FindAsync(supplierId);
            Assert.NotNull(deletedSupplier);
            Assert.True(deletedSupplier.IsDeleted);
        }
        
        [Fact]
        public async Task DeleteSupplierAsync_SupplierNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                supplierService.DeleteSupplierAsync(Guid.NewGuid()));
        }
        
        [Fact]
        public async Task DeleteSupplierAsync_SupplierHasRawMaterials_ThrowsSupplierHasRawMaterialsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add supplier with raw material
            var (supplierId, _) = await SeedSupplierWithRawMaterialAsync(context);
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
            
            // Act & Assert
            await Assert.ThrowsAsync<SupplierHasRawMaterialsException>(() => 
                supplierService.DeleteSupplierAsync(supplierId));
        }
        
        [Fact]
        public async Task GetSupplierByIdAsync_ExistingSupplier_ReturnsSupplierDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add existing supplier
            var supplierId = await SeedSupplierAsync(context, "Test Supplier", "TX123456");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
            
            // Act
            var result = await supplierService.GetSupplierByIdAsync(supplierId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplierId, result.Id);
            Assert.Equal("Test Supplier", result.Name);
            Assert.Equal("TX123456", result.TaxNumber);
            Assert.Equal(_companyId, result.CompanyId);
        }
        
        [Fact]
        public async Task GetSupplierByIdAsync_SupplierNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
            
            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                supplierService.GetSupplierByIdAsync(Guid.NewGuid()));
        }
        
        [Fact]
        public async Task GetSuppliersAsync_ReturnsPagedResult()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add multiple suppliers
            for (int i = 1; i <= 10; i++)
            {
                await SeedSupplierAsync(context, $"Supplier {i}", $"TX{i:D6}");
            }
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 5,
                Search = "",
                OrderBy = "Name",
                IsDesc = false
            };
            
            // Act
            var result = await supplierService.GetSuppliersAsync(paginationRequest);
            
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
        public async Task GetSuppliersAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Add suppliers with different names
            await SeedSupplierAsync(context, "Apple Supplier", "TX111111");
            await SeedSupplierAsync(context, "Banana Supplier", "TX222222");
            await SeedSupplierAsync(context, "Cherry Supplier", "TX333333");
            
            var supplierService = new SupplierService(
                context, 
                _currentUserServiceMock.Object, 
                _localizationServiceMock.Object, 
                _mapper);
                
            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "ban", // Should match "Banana Supplier"
                OrderBy = "Name",
                IsDesc = false
            };
            
            // Act
            var result = await supplierService.GetSuppliersAsync(paginationRequest);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Banana Supplier", result.Items[0].Name);
        }
    }
} 