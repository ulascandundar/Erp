using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities.NoSqlEntities;

/// <summary>
/// Hammadde tüketim raporu için veri sınıfı
/// </summary>
public class RawMaterialConsumption
{
	public Guid RawMaterialId { get; set; }
	public string RawMaterialName { get; set; }
	public Guid UnitId { get; set; }
	public string UnitName { get; set; }
	public decimal ConsumedQuantity { get; set; }
	public decimal UnitPrice { get; set; }
	public decimal TotalCost { get; set; }
}