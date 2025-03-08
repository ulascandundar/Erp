using Erp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Erp.Socket.Hubs;

[Authorize] // JWT token'ı olan kullanıcılar bağlanabilir
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Kullanıcı bağlandığında çalışacak kod
        var userId = Context.User.FindFirst(CustomClaims.Id)?.Value;
        Console.WriteLine($"Bağlantı kuruldu. Kullanıcı ID: {userId ?? "Bulunamadı"}");
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Kullanıcıyı kendi ID'sine göre bir gruba ekle
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            
            // Kullanıcının rolüne göre gruplara ekleyebiliriz
            if (Context.User.IsInRole(CustomClaims.CompanyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "CompanyAdmins");
            }
            
            // Kullanıcının şirket ID'sine göre gruba ekle
            var companyIdClaim = Context.User.FindFirst(CustomClaims.CompanyId)?.Value;
            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, companyId.ToString());
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Kullanıcı bağlantısı kesildiğinde çalışacak kod
        var userId = Context.User.FindFirst(CustomClaims.Id)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            
            if (Context.User.IsInRole(CustomClaims.CompanyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "CompanyAdmins");
            }
            // Kullanıcının şirket ID'sine göre gruptan çıkar
            var companyIdClaim = Context.User.FindFirst(CustomClaims.Id)?.Value;
            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, companyId.ToString());
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    // Client'ların çağırabileceği metotlar
    public async Task SendMessageTest(string message)
    {
        var userId = Context.User.FindFirst(CustomClaims.Id)?.Value;
        // Client'lara mesaj gönder
        //await Clients.All.SendAsync("ReceiveMessage", userId, message);
    }
} 