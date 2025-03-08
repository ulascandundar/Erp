using Erp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class Unit : BaseEntity
{
	public string Name { get; set; }
	public Guid? CompanyId { get; set; }
	public Company Company { get; set; }
	public bool IsGlobal { get; set; }
	public UnitType UnitType { get; set; }
}
