using Erp.Domain.Interfaces;
using Erp.Socket.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Erp.Socket.Services;

public class NotificationHubService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToAllAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendToUserAsync(string userId, string message)
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendToCompanyAdminsAsync(string message)
    {
        await _hubContext.Clients.Group("CompanyAdmins").SendAsync("ReceiveNotification", message);
    }

    public async Task SendToCompanyAsync(Guid companyId, string message)
    {
        await _hubContext.Clients.Group(companyId.ToString()).SendAsync("ReceiveNotification", message);
    }

    public async Task SendToGroupAsync(string groupName, string message)
    {
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", message);
    }
} 