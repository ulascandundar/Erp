using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UnitHasProductRawMaterialException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitHasProductRawMaterialException() 
        : base(ResourceKeys.Errors.UnitHasProductRawMaterial) { }
        
    public UnitHasProductRawMaterialException(string message) 
        : base(message) { }
} 