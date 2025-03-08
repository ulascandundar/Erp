using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class RawMaterial : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
	public string Barcode { get; set; }
	public Guid CompanyId { get; set; }
	public Company Company { get; set; }
}
