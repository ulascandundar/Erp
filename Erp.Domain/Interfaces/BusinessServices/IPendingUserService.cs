using Erp.Domain.DTOs.Auth;
using Erp.Domain.DTOs.PenginUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IPendingUserService
{
	Task<PendingUserResponseDto> CreateUserAsync(PendingUserCreateDto pendingUserCreateDto);
	Task<TokenDto> VerifyOtpAsync(PendingUserVerifyOtpDto pendingUserVerifyOtpDto);
}
