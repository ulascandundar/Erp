using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.UnitServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
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

namespace Erp.Application.Tests.Services.UnitServices
{
    public class UnitServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public UnitServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
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
        public async Task CreateUnitAsync_ValidUnit_ReturnsUnitDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            await context.Units.AddAsync(rootUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var unitCreateDto = new UnitCreateDto
            {
                Name = "Test Unit",
                ShortCode = "TST",
                Description = "Test Unit Description",
                ConversionRate = 2,
                RootUnitId = rootUnit.Id
            };

            // Act
            var result = await unitService.CreateUnitAsync(unitCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(unitCreateDto.Name, result.Name);
            Assert.Equal(unitCreateDto.ShortCode, result.ShortCode);
            Assert.Equal(unitCreateDto.Description, result.Description);
            Assert.Equal(unitCreateDto.ConversionRate, result.ConversionRate);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(rootUnit.UnitType, result.UnitType);
        }

        [Fact]
        public async Task CreateUnitAsync_DuplicateName_ThrowsUnitNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Aynı isimli birim oluştur
            var existingUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Test Unit",
                ShortCode = "TST1",
                Description = "Existing Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, existingUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var unitCreateDto = new UnitCreateDto
            {
                Name = "Test Unit", // Aynı isim
                ShortCode = "TST2", // Farklı kısa kod
                Description = "Test Unit Description",
                ConversionRate = 3,
                RootUnitId = rootUnit.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnitNameAlreadyExistsException>(() => unitService.CreateUnitAsync(unitCreateDto));
        }

        [Fact]
        public async Task CreateUnitAsync_DuplicateShortCode_ThrowsUnitShortCodeAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Aynı kısa kodlu birim oluştur
            var existingUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Existing Unit",
                ShortCode = "TST",
                Description = "Existing Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, existingUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var unitCreateDto = new UnitCreateDto
            {
                Name = "Test Unit 2", // Farklı isim
                ShortCode = "TST", // Aynı kısa kod
                Description = "Test Unit Description",
                ConversionRate = 3,
                RootUnitId = rootUnit.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnitShortCodeAlreadyExistsException>(() => unitService.CreateUnitAsync(unitCreateDto));
        }

        [Fact]
        public async Task CreateUnitAsync_NoCompanyId_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            await context.Units.AddAsync(rootUnit);
            await context.SaveChangesAsync();
            
            // Şirketi olmayan kullanıcı
            var currentUserWithoutCompany = new UserDto
            {
                Id = _userId,
                CompanyId = null
            };
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(currentUserWithoutCompany);
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, currentUserServiceMock.Object, _mapper);
            
            var unitCreateDto = new UnitCreateDto
            {
                Name = "Test Unit",
                ShortCode = "TST",
                Description = "Test Unit Description",
                ConversionRate = 2,
                RootUnitId = rootUnit.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => unitService.CreateUnitAsync(unitCreateDto));
        }

        [Fact]
        public async Task UpdateUnitAsync_ValidUnit_ReturnsUpdatedUnitDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Güncellenecek birim oluştur
            var existingUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Existing Unit",
                ShortCode = "EXS",
                Description = "Existing Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, existingUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var unitUpdateDto = new UnitUpdateDto
            {
                Name = "Updated Unit",
                ShortCode = "UPD",
                Description = "Updated Unit Description",
                ConversionRate = 3,
                RootUnitId = rootUnit.Id
            };

            // Act
            var result = await unitService.UpdateUnitAsync(existingUnit.Id, unitUpdateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(unitUpdateDto.Name, result.Name);
            Assert.Equal(unitUpdateDto.ShortCode, result.ShortCode);
            Assert.Equal(unitUpdateDto.Description, result.Description);
            Assert.Equal(unitUpdateDto.ConversionRate, result.ConversionRate);
            Assert.Equal(_companyId, result.CompanyId);
            Assert.Equal(rootUnit.UnitType, result.UnitType);
        }

        [Fact]
        public async Task UpdateUnitAsync_UnitNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);
            
            var unitUpdateDto = new UnitUpdateDto
            {
                Name = "Updated Unit",
                ShortCode = "UPD",
                Description = "Updated Unit Description",
                ConversionRate = 3,
                RootUnitId = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => unitService.UpdateUnitAsync(Guid.NewGuid(), unitUpdateDto));
        }

        [Fact]
        public async Task DeleteAsync_ValidUnit_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Silinecek birim oluştur
            var unitToDelete = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Unit To Delete",
                ShortCode = "DEL",
                Description = "Unit To Delete Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, unitToDelete);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            await unitService.DeleteAsync(unitToDelete.Id);

            // Assert
            var deletedUnit = await context.Units.FindAsync(unitToDelete.Id);
            Assert.NotNull(deletedUnit);
            Assert.True(deletedUnit.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_UnitNotFound_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => unitService.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_UnitHasChildUnit_ThrowsUnitHasChildUnitException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Alt birim oluştur
            var childUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Child Unit",
                ShortCode = "CHD",
                Description = "Child Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, childUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act & Assert
            await Assert.ThrowsAsync<UnitHasChildUnitException>(() => unitService.DeleteAsync(rootUnit.Id));
        }

        [Fact]
        public async Task ConvertUnit_SameUnit_ReturnsOne()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Birim oluştur
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Test Unit",
                ShortCode = "TST",
                Description = "Test Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Hammadde oluştur
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                UnitId = unit.Id,
                CompanyId = _companyId
            };
            
            await context.Units.AddAsync(unit);
            await context.RawMaterials.AddAsync(rawMaterial);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            var result = await unitService.ConvertUnit(unit.Id, rawMaterial.Id);

            // Assert
            Assert.Equal(1m, result);
        }

        [Fact]
        public async Task ConvertUnit_DifferentUnits_ReturnsCorrectRate()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Kaynak birim oluştur (2 rootUnit)
            var sourceUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Source Unit",
                ShortCode = "SRC",
                Description = "Source Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            // Hedef birim oluştur (5 rootUnit)
            var targetUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Target Unit",
                ShortCode = "TRG",
                Description = "Target Unit Description",
                ConversionRate = 5,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            // Hammadde oluştur
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                UnitId = sourceUnit.Id,
                CompanyId = _companyId
            };
            
            await context.Units.AddRangeAsync(rootUnit, sourceUnit, targetUnit);
            await context.RawMaterials.AddAsync(rawMaterial);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            var result = await unitService.ConvertUnit(targetUnit.Id, rawMaterial.Id);

            // Assert
            // sourceUnit = 2 rootUnit, targetUnit = 5 rootUnit
            // 2/5 = 0.4 (sourceUnit/targetUnit)
            Assert.Equal(0.4m, result);
        }

        [Fact]
        public async Task ConvertUnit_DifferentUnitTypes_ThrowsUnitTypeMismatchException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Ağırlık birimi oluştur
            var weightUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Weight Unit",
                ShortCode = "WGT",
                Description = "Weight Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Hacim birimi oluştur
            var volumeUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Volume Unit",
                ShortCode = "VOL",
                Description = "Volume Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Volume,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Hammadde oluştur
            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = "Test Raw Material",
                UnitId = weightUnit.Id,
                CompanyId = _companyId
            };
            
            await context.Units.AddRangeAsync(weightUnit, volumeUnit);
            await context.RawMaterials.AddAsync(rawMaterial);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act & Assert
            await Assert.ThrowsAsync<UnitTypeMismatchException>(() => unitService.ConvertUnit(volumeUnit.Id, rawMaterial.Id));
        }

        [Fact]
        public async Task FindRateToRootAsync_ReturnsCorrectRate()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Kök birim oluştur
            var rootUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Root Unit",
                ShortCode = "ROOT",
                Description = "Root Unit Description",
                ConversionRate = 1,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                IsGlobal = false
            };
            
            // Orta birim oluştur (2 rootUnit)
            var middleUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Middle Unit",
                ShortCode = "MID",
                Description = "Middle Unit Description",
                ConversionRate = 2,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = rootUnit.Id,
                IsGlobal = false
            };
            
            // Alt birim oluştur (3 middleUnit = 6 rootUnit)
            var childUnit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Child Unit",
                ShortCode = "CHD",
                Description = "Child Unit Description",
                ConversionRate = 3,
                UnitType = UnitType.Weight,
                CompanyId = _companyId,
                RootUnitId = middleUnit.Id,
                IsGlobal = false
            };
            
            await context.Units.AddRangeAsync(rootUnit, middleUnit, childUnit);
            await context.SaveChangesAsync();
            
            var unitService = new UnitService(context, _localizationServiceMock.Object, _currentUserServiceMock.Object, _mapper);

            // Act
            var result = await unitService.FindRateToRootAsync(childUnit.Id);

            // Assert
            // childUnit = 3 middleUnit, middleUnit = 2 rootUnit
            // 3 * 2 = 6 (childUnit to rootUnit)
            Assert.Equal(6m, result);
        }
    }
} 