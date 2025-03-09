using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;
using Erp.Domain.Interfaces.BusinessServices;

namespace Erp.Domain.CustomExceptions;

public class UnitTypeMismatchException : NonLoggableException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.OK;
    
    public UnitTypeMismatchException(ILocalizationService localizationService) 
        : base(localizationService.GetLocalizedString(ResourceKeys.Errors.UnitTypeMismatch)) { }
        
    public UnitTypeMismatchException(string message) 
        : base(message) { }
} 