using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Enums;

public enum UnitType
{
	[Display(Name = "Ağırlık")] // gram
	Weight = 1,
	[Display(Name = "Hacim")] //mililitre
	Volume = 2,
	[Display(Name = "Uzunluk")] // cm
	Length = 3,
	[Display(Name = "Adet")] // 10 lu kutu
	Count = 4,
	[Display(Name = "Alan")] // cm2
	Area = 5,
	[Display(Name = "Zaman")] // saniye
	Time = 6
}
