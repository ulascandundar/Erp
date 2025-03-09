using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UnitHasChildUnitException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitHasChildUnitException() 
        : base(ResourceKeys.Errors.UnitHasChildUnit) { }
        
    public UnitHasChildUnitException(string message) 
        : base(message) { }
} 