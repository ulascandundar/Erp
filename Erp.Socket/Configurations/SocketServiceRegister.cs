using Erp.Domain.Interfaces;
using Erp.Socket.Hubs;
using Erp.Socket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Socket.Configurations;

public static class SocketServiceRegister
{
    public static IServiceCollection RegisterSocketServices(this IServiceCollection services)
    {
        // SignalR servisini ekle
        services.AddSignalR();
        
        // NotificationHub servisini kaydet
        services.AddScoped<INotificationHub, NotificationHubService>();
        
        return services;
    }
} 