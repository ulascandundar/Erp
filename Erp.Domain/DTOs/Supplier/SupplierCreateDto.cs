using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Supplier;

public class SupplierCreateDto
{
    public string Name { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string TaxNumber { get; set; }
    public string Website { get; set; }
    public string Description { get; set; }
} 