using AutoMapper;
using Erp.Application.Services.UserServices;
using Erp.Domain.Constants;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Erp.Application.Tests.Services.UserServices
{
    public class UserServiceTests
    {
        private readonly DbContextOptions<ErpDbContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<ILocalizationService> _localizationServiceMock;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public UserServiceTests()
        {
            // InMemory veritabanı oluştur
            _dbContextOptions = new DbContextOptionsBuilder<ErpDbContext>()
                .UseInMemoryDatabase(databaseName: $"ErpTestDb_{Guid.NewGuid()}")
                .Options;

            // AutoMapper yapılandırması
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                // User entity -> UserDto
                cfg.CreateMap<User, UserDto>()
                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));

                // UserCreateDto -> User
                cfg.CreateMap<UserCreateDto, User>()
                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

                // UserUpdateDto -> User
                cfg.CreateMap<UserUpdateDto, User>()
                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
            });
            _mapper = mappingConfig.CreateMapper();

            // HttpContextAccessor mock'u
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // PasswordHasher mock'u
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                .Returns("hashedpassword");

            // CurrentUserService mock'u
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            var currentUser = new UserDto
            {
                Id = _userId,
                Email = "test@example.com",
                CompanyId = _companyId,
                Roles = new List<string> { "Admin" }
            };
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).Returns(currentUser);
            
            // LocalizationService mock'u
            _localizationServiceMock = new Mock<ILocalizationService>();
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.UserNotFound))
                .Returns("Kullanıcı bulunamadı");
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.UserNotBelongToCompany))
                .Returns("Kullanıcı bir şirkete bağlı değil");
            _localizationServiceMock.Setup(x => x.GetLocalizedString(ResourceKeys.Errors.EmailAlreadyExists))
                .Returns("Bu e-posta adresi zaten kullanılmaktadır");
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
        public async Task CreateUserAsync_ValidUser_ReturnsUserDto()
        {
            // Arrange
            using var context = CreateDbContext();
            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var userCreateDto = new UserCreateDto
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                CompanyId = _companyId,
                Roles = new List<string> { "User" }
            };

            // Act
            var result = await userService.CreateUserAsync(userCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userCreateDto.Email, result.Email);
            Assert.Equal(userCreateDto.CompanyId, result.CompanyId);
            Assert.Equal(userCreateDto.Roles, result.Roles);
            
            // Veritabanında kullanıcının oluşturulduğunu doğrula
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == userCreateDto.Email);
            Assert.NotNull(userInDb);
            Assert.Equal("hashedpassword", userInDb.PasswordHash);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateEmail_ThrowsUserEmailAlreadyExistsException()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına aynı e-posta ile bir kullanıcı ekle
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "User" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var userCreateDto = new UserCreateDto
            {
                Email = "existing@example.com", // Aynı e-posta
                Password = "Password123!",
                CompanyId = _companyId,
                Roles = new List<string> { "User" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserEmailAlreadyExistsException>(() => 
                userService.CreateUserAsync(userCreateDto));
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllNonDeletedUsers()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Veritabanına kullanıcılar ekle
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "Admin" },
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user2@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "User" },
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "deleted@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "User" },
                    CompanyId = _companyId,
                    IsDeleted = false // Silinmiş kullanıcı
                }
            };
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            var selectedForRemoveUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "deleted@example.com");
            context.Users.Remove(selectedForRemoveUser);
            await context.SaveChangesAsync();
			var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            var result = await userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Sadece silinmemiş kullanıcılar
            Assert.Contains(result, u => u.Email == "user1@example.com");
            Assert.Contains(result, u => u.Email == "user2@example.com");
            Assert.DoesNotContain(result, u => u.Email == "deleted@example.com"); // Silinmiş kullanıcı
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUserDto()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "Admin" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            var result = await userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.CompanyId, result.CompanyId);
            Assert.Equal(user.Roles, result.Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                userService.GetUserByIdAsync(nonExistingId));
        }

        [Fact]
        public async Task SoftDeleteUserAsync_ExistingUser_SetsIsDeletedToTrue()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "Admin" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            await userService.SoftDeleteUserAsync(userId);

            // Assert
            var deletedUser = await context.Users.FindAsync(userId);
            Assert.True(deletedUser.IsDeleted);
        }

        [Fact]
        public async Task SoftDeleteUserAsync_NonExistingUser_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                userService.SoftDeleteUserAsync(nonExistingId));
        }

        [Fact]
        public async Task HardDeleteUserAsync_ExistingUser_RemovesUserFromDatabase()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "Admin" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            // Act
            await userService.HardDeleteUserAsync(userId);

            // Assert
            var deletedUser = await context.Users.FindAsync(userId);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task HardDeleteUserAsync_NonExistingUser_ThrowsNullValueException()
        {
            // Arrange
            using var context = CreateDbContext();
            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NullValueException>(() => 
                userService.HardDeleteUserAsync(nonExistingId));
        }

        [Fact]
        public async Task ChangePasswordUserAsync_ExistingUser_UpdatesPassword()
        {
            // Arrange
            using var context = CreateDbContext();
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "oldhash",
                Roles = new List<string> { "Admin" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var changePasswordDto = new ChangePasswordUserDto
            {
                Password = "NewPassword123!"
            };

            // Act
            var result = await userService.ChangePasswordUserAsync(changePasswordDto, userId);

            // Assert
            Assert.NotNull(result);
            
            // Veritabanında şifrenin güncellendiğini doğrula
            var updatedUser = await context.Users.FindAsync(userId);
            Assert.Equal("hashedpassword", updatedUser.PasswordHash);
        }

        [Fact]
        public async Task ChangeCurrentUserPasswordAsync_UpdatesCurrentUserPassword()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Mevcut kullanıcıyı veritabanına ekle
            var currentUser = new User
            {
                Id = _userId,
                Email = "test@example.com",
                PasswordHash = "oldhash",
                Roles = new List<string> { "Admin" },
                CompanyId = _companyId,
                IsDeleted = false
            };
            
            context.Users.Add(currentUser);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var changePasswordDto = new ChangePasswordUserDto
            {
                Password = "NewPassword123!"
            };

            // Act
            var result = await userService.ChangeCurrentUserPasswordAsync(changePasswordDto);

            // Assert
            Assert.NotNull(result);
            
            // Veritabanında şifrenin güncellendiğini doğrula
            var updatedUser = await context.Users.FindAsync(_userId);
            Assert.Equal("hashedpassword", updatedUser.PasswordHash);
        }

        [Fact]
        public async Task GedPagedAsync_ReturnsPagedResult()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // 20 kullanıcı ekle
            var users = new List<User>();
            for (int i = 1; i <= 20; i++)
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"user{i}@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "User" },
                    CompanyId = _companyId,
                    IsDeleted = false
                });
            }
            
            // Silinmiş bir kullanıcı ekle
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "deleted@example.com",
                PasswordHash = "hashedpassword",
                Roles = new List<string> { "User" },
                CompanyId = _companyId,
                IsDeleted = true
            });
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            var selectedForRemoveUser = await context.Users.FirstOrDefaultAsync(o => o.Email == "deleted@example.com");
            selectedForRemoveUser.IsDeleted = true;
            context.Users.Update(selectedForRemoveUser);
			await context.SaveChangesAsync();
            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var paginationRequest = new PaginationRequest
            {
                PageNumber = 2,
                PageSize = 5
            };

            // Act
            var result = await userService.GedPagedAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count); // Sayfa başına 5 öğe
            Assert.Equal(20, result.TotalCount); // Toplam 20 kullanıcı (silinmiş kullanıcı hariç)
            Assert.Equal(4, result.TotalPages); // 20/5 = 4 sayfa
            Assert.Equal(2, result.PageNumber); // 2. sayfa
            Assert.Equal(5, result.PageSize); // Sayfa başına 5 öğe
        }

        [Fact]
        public async Task GedPagedAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            using var context = CreateDbContext();
            
            // Farklı e-postalarla kullanıcılar ekle
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "Admin" },
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user@example.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "User" },
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "manager@company.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "Manager" },
                    CompanyId = _companyId,
                    IsDeleted = false
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@company.com",
                    PasswordHash = "hashedpassword",
                    Roles = new List<string> { "Admin" },
                    CompanyId = _companyId,
                    IsDeleted = false
                }
            };
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _passwordHasherMock.Object, _mapper, _currentUserServiceMock.Object, _localizationServiceMock.Object);

            var paginationRequest = new PaginationRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Search = "admin"
            };

            // Act
            var result = await userService.GedPagedAsync(paginationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count); // "admin" içeren 2 kullanıcı
            Assert.Contains(result.Items, u => u.Email == "admin@example.com");
            Assert.Contains(result.Items, u => u.Email == "admin@company.com");
            Assert.DoesNotContain(result.Items, u => u.Email == "user@example.com");
            Assert.DoesNotContain(result.Items, u => u.Email == "manager@company.com");
        }
    }
} 