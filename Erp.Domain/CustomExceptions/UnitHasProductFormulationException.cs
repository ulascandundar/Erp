using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UnitHasProductFormulationException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitHasProductFormulationException() 
        : base(ResourceKeys.Errors.UnitHasProductFormulation) { }
        
    public UnitHasProductFormulationException(string message) 
        : base(message) { }
} 