using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.User;

public class UserCreateDto
{
	public string Email { get; set; }
	public string Password { get; set; }
	public List<string> Roles { get; set; }
}
