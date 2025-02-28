using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class LoggableException : BaseException
{
	public LoggableException(string message) : base(message) { }
}
