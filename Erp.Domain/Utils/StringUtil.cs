using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Utils;

public static class StringUtil
{
	public static string GenerateRandomOtpCode()
	{
		var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		var random = new Random();
		var password = new string(Enumerable.Repeat(chars, 6)
			.Select(s => s[random.Next(s.Length)])
			.ToArray());
		return password;
	}
}
