using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Auth;

public class TokenDto
{
	public string Token { get; set; }
	public DateTime Expiration { get; set; }
	public List<string> Roles { get; set; }
	public string Email { get; set; }
	public Guid? CompanyId { get; set; }
}
