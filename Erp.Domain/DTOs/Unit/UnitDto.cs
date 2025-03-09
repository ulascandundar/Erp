using Erp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Unit;

public class UnitDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string ShortCode { get; set; }
	public decimal ConversionRate { get; set; }
	public Guid? RootUnitId { get; set; }
	public Guid? CompanyId { get; set; }
	public bool IsGlobal { get; set; }
	public UnitType UnitType { get; set; }
	public decimal RateToRoot { get; set; }
}
