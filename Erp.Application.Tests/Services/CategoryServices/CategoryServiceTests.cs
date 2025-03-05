using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.CategoryServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Category;
using Erp.Domain.DTOs.Pagination;
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

namespace Erp.Application.Tests.Services.CategoryServices
{
    public class CategoryServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public CategoryServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryMappingProfile());
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
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.CategoryNotFound))
                .Returns("Kategori bulunamadı");
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany))
                .Returns("Kullanıcı bir şirkete bağlı değil");
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.CategoryNameAlreadyExists))
                .Returns("Bu kategori adı şirketinizde zaten kullanılmaktadır");
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
        public async Task CreateCategoryAsync_ValidCategory_ReturnsCategoryDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var categoryCreateDto = new CategoryCreateDto
            {
                Name = "Test Category",
                Description = "Test Description"
            };

            // Act
            var result = await categoryService.CreateCategoryAsync(categoryCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryCreateDto.Name, result.Name);
            Assert.Equal(categoryCreateDto.Description, result.Description);
            Assert.Equal(_companyId, result.CompanyId);
        }

        [Fact]
        public async Task CreateCategoryAsync_DuplicateName_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına aynı isimde bir kategori ekle
            context.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Existing Description",
                CompanyId = _companyId,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var categoryCreateDto = new CategoryCreateDto
            {
                Name = "Test Category", // Aynı isim
                Description = "New Description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                categoryService.CreateCategoryAsync(categoryCreateDto));
        }

        [Fact]
        public async Task CreateCategoryAsync_NoCompanyId_ThrowsNullValueException()
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

            var categoryService = new CategoryService(context, _mapper, currentUserServiceMock.Object, _localizationServiceMock.Object);

            var categoryCreateDto = new CategoryCreateDto
            {
                Name = "Test Category",
                Description = "Test Description"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullValueException>(() => 
                categoryService.CreateCategoryAsync(categoryCreateDto));
            
            Assert.Equal("Kullanıcı bir şirkete bağlı değil", exception.Message);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategoriesForCompany()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına kategoriler ekle
            var categories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Category 1",
                    Description = "Description 1",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Category 2",
                    Description = "Description 2",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Category 3",
                    Description = "Description 3",
                    CompanyId = Guid.NewGuid(), // Farklı şirket
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Category 4",
                    Description = "Description 4",
                    CompanyId = _companyId,
                    IsDeleted = false // Silinmiş
                }
            };
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            var selectedForRemoveCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Category 4");
			context.Categories.Remove(selectedForRemoveCategory);
			await context.SaveChangesAsync();
			var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            var result = await categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Sadece kullanıcının şirketine ait ve silinmemiş kategoriler
            Assert.Contains(result, c => c.Name == "Category 1");
            Assert.Contains(result, c => c.Name == "Category 2");
            Assert.DoesNotContain(result, c => c.Name == "Category 3"); // Farklı şirket
            Assert.DoesNotContain(result, c => c.Name == "Category 4"); // Silinmiş
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ExistingCategory_ReturnsCategoryDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Description = "Test Description",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            var result = await categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Description, result.Description);
            Assert.Equal(category.CompanyId, result.CompanyId);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_NonExistingCategory_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                categoryService.GetCategoryByIdAsync(nonExistingId));
        }

        [Fact]
        public async Task UpdateCategoryAsync_ValidCategory_ReturnsUpdatedCategoryDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = "Original Category",
                Description = "Original Description",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var updateDto = new CategoryUpdateDto
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            // Act
            var result = await categoryService.UpdateCategoryAsync(updateDto, categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            
            // Veritabanında da güncellendiğini kontrol et
            var updatedCategory = await context.Categories.FindAsync(categoryId);
            Assert.Equal(updateDto.Name, updatedCategory.Name);
            Assert.Equal(updateDto.Description, updatedCategory.Description);
            Assert.NotNull(updatedCategory.UpdatedAt);
        }

        [Fact]
        public async Task UpdateCategoryAsync_DuplicateName_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // İki farklı kategori ekle
            var categoryId1 = Guid.NewGuid();
            var categoryId2 = Guid.NewGuid();
            
            context.Categories.AddRange(
                new Category
                {
                    Id = categoryId1,
                    Name = "Category 1",
                    Description = "Description 1",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = categoryId2,
                    Name = "Category 2",
                    Description = "Description 2",
                    CompanyId = _companyId,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Category 2'yi Category 1 ile aynı isimle güncellemeye çalış
            var updateDto = new CategoryUpdateDto
            {
                Name = "Category 1", // Zaten var olan isim
                Description = "Updated Description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                categoryService.UpdateCategoryAsync(updateDto, categoryId2));
        }

        [Fact]
        public async Task SoftDeleteCategoryAsync_ExistingCategory_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Description = "Test Description",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            await categoryService.SoftDeleteCategoryAsync(categoryId);

            // Assert
            var deletedCategory = await context.Categories.FindAsync(categoryId);
            Assert.True(deletedCategory.IsDeleted);
        }

        [Fact]
        public async Task HardDeleteCategoryAsync_ExistingCategory_RemovesCategoryFromDatabase()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                Description = "Test Description",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            await categoryService.HardDeleteCategoryAsync(categoryId);

            // Assert
            var deletedCategory = await context.Categories.FindAsync(categoryId);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // 20 kategori ekle
            var categories = new List<Category>();
            for (int i = 1; i <= 20; i++)
            {
                categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = $"Category {i}",
                    Description = $"Description {i}",
                    CompanyId = _companyId,
                    IsDeleted = false
                });
            }
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var paginationRequest = new PaginationRequest
            {
                PageNumber = 2,
                PageSize = 5
            };

            // Act
            var result = await categoryService.GetPagedAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count); // Sayfa başına 5 öğe
            Assert.Equal(20, result.TotalCount); // Toplam 20 kategori
            Assert.Equal(4, result.TotalPages); // 20/5 = 4 sayfa
            Assert.Equal(2, result.PageNumber); // 2. sayfa
            Assert.Equal(5, result.PageSize); // Sayfa başına 5 öğe
        }

        [Fact]
        public async Task GetPagedAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Farklı isimlerle kategoriler ekle
            var categories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Apple Category",
                    Description = "Fruit Description",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Banana Category",
                    Description = "Yellow Fruit",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Car Category",
                    Description = "Vehicle Description",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Desk Category",
                    Description = "Furniture with Apple design",
                    CompanyId = _companyId,
                    IsDeleted = false
                }
            };
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var categoryService = new CategoryService(context, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "Apple"
            };

            // Act
            var result = await categoryService.GetPagedAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count); // "Apple" içeren 2 kategori
            Assert.Contains(result.Items, c => c.Name == "Apple Category");
            Assert.Contains(result.Items, c => c.Description.Contains("Apple"));
            Assert.DoesNotContain(result.Items, c => c.Name == "Banana Category");
            Assert.DoesNotContain(result.Items, c => c.Name == "Car Category");
        }
    }
} 