using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class NonLoggableException : BaseException
{
	public NonLoggableException(string message) : base(message) { }
}
