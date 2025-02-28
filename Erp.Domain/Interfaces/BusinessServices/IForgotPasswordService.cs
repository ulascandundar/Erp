using Erp.Domain.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IForgotPasswordService
{
	Task<string> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
	Task VerifyOtpAsync(ForgotPasswordWithPasswordDto forgotPasswordWithPasswordDto);
}
