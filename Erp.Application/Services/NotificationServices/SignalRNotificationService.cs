using Erp.Domain.Interfaces;
using Erp.Domain.Interfaces.BusinessServices;
using System;
using System.Threading.Tasks;

namespace Erp.Application.Services.NotificationServices;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly INotificationHub _notificationHub;
    private readonly ICurrentUserService _currentUserService;

    public SignalRNotificationService(
        INotificationHub notificationHub,
        ICurrentUserService currentUserService)
    {
        _notificationHub = notificationHub;
        _currentUserService = currentUserService;
    }

    public async Task SendNotificationToAllAsync(string message)
    {
        await _notificationHub.SendToAllAsync(message);
    }

    public async Task SendNotificationToUserAsync(string userId, string message)
    {
        await _notificationHub.SendToUserAsync(userId, message);
    }

    public async Task SendNotificationToCompanyAdminsAsync(string message)
    {
        await _notificationHub.SendToCompanyAdminsAsync(message);
    }

    public async Task SendNotificationToCompanyAsync(Guid companyId, string message)
    {
        await _notificationHub.SendToCompanyAsync(companyId, message);
    }
} 