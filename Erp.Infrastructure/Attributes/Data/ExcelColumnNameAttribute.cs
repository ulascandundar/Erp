using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Attributes.Data;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelColumnNameAttribute : Attribute
{
	public string ColumnName { get; }

	public ExcelColumnNameAttribute(string columnName)
	{
		ColumnName = columnName;
	}
}
