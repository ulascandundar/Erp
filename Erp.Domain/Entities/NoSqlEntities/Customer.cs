﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities.NoSqlEntities;

public class Customer
{
	public string Tckn { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string PhoneNumber { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
}
