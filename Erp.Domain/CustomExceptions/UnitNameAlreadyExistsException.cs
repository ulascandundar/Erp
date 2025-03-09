using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UnitNameAlreadyExistsException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitNameAlreadyExistsException() 
        : base(ResourceKeys.Errors.UnitNameAlreadyExists) { }
        
    public UnitNameAlreadyExistsException(string message) 
        : base(message) { }
} 