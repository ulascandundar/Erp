using Erp.Domain.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces;

public interface IAuthService
{
	Task<TokenDto> LoginAsync(UserLoginDto userLoginDto);
}
