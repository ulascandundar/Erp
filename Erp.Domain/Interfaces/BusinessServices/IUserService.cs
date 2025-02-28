using Erp.Domain.DTOs.Auth;
using Erp.Domain.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IUserService
{
	Task<List<UserDto>> GetAllUsersAsync();

	Task<UserDto> GetUserByIdAsync(Guid id);

	Task HardDeleteUserAsync(Guid id);

	Task SoftDeleteUserAsync(Guid id);

	Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto);
}
