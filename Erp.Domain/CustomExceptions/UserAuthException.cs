using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class UserAuthException : NonLoggableException
{
	public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
	public UserAuthException(string message) : base(message) { }
}
