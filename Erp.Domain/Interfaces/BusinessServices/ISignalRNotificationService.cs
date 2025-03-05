using System;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface ISignalRNotificationService
{
    Task SendNotificationToAllAsync(string message);
    Task SendNotificationToUserAsync(string userId, string message);
    Task SendNotificationToCompanyAdminsAsync(string message);
    Task SendNotificationToCompanyAsync(Guid companyId, string message);
} 