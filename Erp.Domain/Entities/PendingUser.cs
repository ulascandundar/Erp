using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class PendingUser : BaseEntity
{
	public string Tckn { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string PhoneNumber { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Otp { get; set; }
	public string Password { get; set; }
	public DateTime ExpiryDate { get; set; }
	public bool IsUsed { get; set; }
}
