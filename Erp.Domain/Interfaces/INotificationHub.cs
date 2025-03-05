using System;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces;

/// <summary>
/// SignalR hub için arayüz. Bu arayüz, Application katmanının API katmanına bağımlı olmasını önler.
/// </summary>
public interface INotificationHub
{
    Task SendToAllAsync(string message);
    Task SendToUserAsync(string userId, string message);
    Task SendToCompanyAdminsAsync(string message);
    Task SendToCompanyAsync(Guid companyId, string message);
    Task SendToGroupAsync(string groupName, string message);
} 