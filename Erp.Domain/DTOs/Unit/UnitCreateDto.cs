using Erp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Unit;

public class UnitCreateDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string ShortCode { get; set; }
	public decimal ConversionRate { get; set; }
	public Guid RootUnitId { get; set; }
}
