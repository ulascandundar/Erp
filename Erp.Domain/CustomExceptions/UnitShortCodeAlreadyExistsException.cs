using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UnitShortCodeAlreadyExistsException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitShortCodeAlreadyExistsException() 
        : base(ResourceKeys.Errors.UnitShortCodeAlreadyExists) { }
        
    public UnitShortCodeAlreadyExistsException(string message) 
        : base(message) { }
} 