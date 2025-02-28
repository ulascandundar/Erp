using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class NullValueException : NonLoggableException
{
	public NullValueException(string message) : base(message) { }
}
