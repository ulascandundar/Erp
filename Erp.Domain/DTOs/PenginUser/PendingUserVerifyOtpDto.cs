using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.PenginUser;

public class PendingUserVerifyOtpDto
{
	public Guid PendingUserId { get; set; }
	public string Otp { get; set; }
}
