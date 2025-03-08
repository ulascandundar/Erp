using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class LoggableException : BaseException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.OK;
	public LoggableException(string message) : base(message) { }
}
