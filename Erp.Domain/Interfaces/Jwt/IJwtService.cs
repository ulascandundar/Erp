using Erp.Domain.DTOs.Auth;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.Jwt;

public interface IJwtService
{
	TokenDto GenerateToken(User user);
	bool ValidateToken(string token);
	IDictionary<string, string> GetClaimsFromToken(string token);
}