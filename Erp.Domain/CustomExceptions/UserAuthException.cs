using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class UserAuthException : LoggableException
{
	public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
	public UserAuthException(string message) : base(message) { }
}
