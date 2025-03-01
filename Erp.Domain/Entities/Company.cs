using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class Company : BaseEntity
{
	public string Name { get; set; }
	public string Address { get; set; }
	public string PhoneNumber { get; set; }
	public string Email { get; set; }
	public string TaxNumber { get; set; }
	public string Website { get; set; }
	public string Description { get; set; }
	public List<User> Users { get; set; }
}
