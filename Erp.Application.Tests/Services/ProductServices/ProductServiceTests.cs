using AutoMapper;
using Erp.Application.Mappings;
using Erp.Application.Services.ProductServices;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Product;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Erp.Application.Tests.Services.ProductServices
{
    public class ProductServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public ProductServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductMappingProfile());
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
        }

        private ErpDbContext CreateDbContext()
        {
            var context = new ErpDbContext(_dbContextOptions, _httpContextAccessorMock.Object);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CreateProductAsync_ValidProduct_ReturnsProductDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            var productCreateDto = new ProductCreateDto
            {
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                Description = "Test Description",
                Price = 100
            };

            // Act
            var result = await productService.CreateProductAsync(productCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productCreateDto.Name, result.Name);
            Assert.Equal(productCreateDto.SKU, result.SKU);
            Assert.Equal(productCreateDto.Barcode, result.Barcode);
            Assert.Equal(productCreateDto.Description, result.Description);
            Assert.Equal(productCreateDto.Price, result.Price);
            Assert.Equal(_companyId, result.CompanyId);
        }

        [Fact]
        public async Task CreateProductAsync_DuplicateName_ThrowsProductNameAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına aynı isimde bir ürün ekle
            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                CompanyId = _companyId,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            var productCreateDto = new ProductCreateDto
            {
                Name = "Test Product", // Aynı isim
                SKU = "TST-002", // Farklı SKU
                Barcode = "1234567890124", // Farklı barkod
                Description = "Test Description",
                Price = 100
            };

            // Act & Assert
            await Assert.ThrowsAsync<ProductNameAlreadyExistsException>(() => 
                productService.CreateProductAsync(productCreateDto));
        }

        [Fact]
        public async Task CreateProductAsync_DuplicateSKU_ThrowsSkuAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına aynı SKU'ya sahip bir ürün ekle
            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                CompanyId = _companyId,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            var productCreateDto = new ProductCreateDto
            {
                Name = "New Product", // Farklı isim
                SKU = "TST-001", // Aynı SKU
                Barcode = "1234567890124", // Farklı barkod
                Description = "Test Description",
                Price = 100
            };

            // Act & Assert
            await Assert.ThrowsAsync<SkuAlreadyExistsException>(() => 
                productService.CreateProductAsync(productCreateDto));
        }

        [Fact]
        public async Task CreateProductAsync_DuplicateBarcode_ThrowsProductBarcodeAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına aynı barkoda sahip bir ürün ekle
            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                CompanyId = _companyId,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            var productCreateDto = new ProductCreateDto
            {
                Name = "New Product", // Farklı isim
                SKU = "TST-002", // Farklı SKU
                Barcode = "1234567890123", // Aynı barkod
                Description = "Test Description",
                Price = 100
            };

            // Act & Assert
            await Assert.ThrowsAsync<ProductBarcodeAlreadyExistsException>(() => 
                productService.CreateProductAsync(productCreateDto));
        }

        [Fact]
        public async Task CreateProductAsync_NoCompanyId_ThrowsNullValueException()
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

            var productService = new ProductService(context, _mapper, currentUserServiceMock.Object);

            var productCreateDto = new ProductCreateDto
            {
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                Description = "Test Description",
                Price = 100
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullValueException>(() => 
                productService.CreateProductAsync(productCreateDto));
            
            Assert.Equal("Kullanıcı bir şirkete bağlı değil", exception.Message);
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProductsForCompany()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına ürünler ekle
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 1",
                    SKU = "SKU-001",
                    Barcode = "1234567890123",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 2",
                    SKU = "SKU-002",
                    Barcode = "1234567890124",
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 3",
                    SKU = "SKU-003",
                    Barcode = "1234567890125",
                    CompanyId = Guid.NewGuid(), // Farklı şirket
                    IsDeleted = false
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 4",
                    SKU = "SKU-004",
                    Barcode = "1234567890126",
                    CompanyId = _companyId,
                    IsDeleted = false // Silinmiş ürün
                }
            };
            
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
            var selectedForRemoveProduct = context.Products.FirstOrDefault(o => o.Name == "Product 4");
            selectedForRemoveProduct.IsDeleted = true;
            context.Products.Update(selectedForRemoveProduct);
            await context.SaveChangesAsync();
            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            // Act
            var result = await productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Sadece kullanıcının şirketine ait ve silinmemiş ürünler
            Assert.Contains(result, p => p.Name == "Product 1");
            Assert.Contains(result, p => p.Name == "Product 2");
            Assert.DoesNotContain(result, p => p.Name == "Product 3"); // Farklı şirket
            Assert.DoesNotContain(result, p => p.Name == "Product 4"); // Silinmiş ürün
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                Description = "Test Description",
                Price = 100,
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            // Act
            var result = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(product.SKU, result.SKU);
            Assert.Equal(product.Barcode, result.Barcode);
            Assert.Equal(product.Description, result.Description);
            Assert.Equal(product.Price, result.Price);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistingProduct_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                productService.GetProductByIdAsync(nonExistingId));
        }

        [Fact]
        public async Task UpdateProductAsync_ValidProduct_ReturnsUpdatedProductDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Original Product",
                SKU = "ORG-001",
                Barcode = "1234567890123",
                Description = "Original Description",
                Price = 100,
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            var updateDto = new ProductUpdateDto
            {
                Name = "Updated Product",
                SKU = "UPD-001",
                Barcode = "9876543210987",
                Description = "Updated Description",
                Price = 200
            };

            // Act
            var result = await productService.UpdateProductAsync(updateDto, productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.SKU, result.SKU);
            Assert.Equal(updateDto.Barcode, result.Barcode);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.Price, result.Price);
            
            // Veritabanında da güncellendiğini kontrol et
            var updatedProduct = await context.Products.FindAsync(productId);
            Assert.Equal(updateDto.Name, updatedProduct.Name);
            Assert.Equal(updateDto.SKU, updatedProduct.SKU);
        }

        [Fact]
        public async Task SoftDeleteProductAsync_ExistingProduct_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            // Act
            await productService.SoftDeleteProductAsync(productId);

            // Assert
            var deletedProduct = await context.Products.FindAsync(productId);
            Assert.True(deletedProduct.IsDeleted);
        }

        [Fact]
        public async Task HardDeleteProductAsync_ExistingProduct_RemovesProductFromDatabase()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                SKU = "TST-001",
                Barcode = "1234567890123",
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var productService = new ProductService(context, _mapper, _currentUserServiceMock.Object);

            // Act
            await productService.HardDeleteProductAsync(productId);

            // Assert
            var deletedProduct = await context.Products.FindAsync(productId);
            Assert.Null(deletedProduct);
        }
    }
} 