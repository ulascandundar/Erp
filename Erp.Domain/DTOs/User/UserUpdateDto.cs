﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.User;

public class UserUpdateDto
{
	public string Email { get; set; }
	public List<string> Roles { get; set; }
}
