using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Notifications.Sender;

public class NotificationSender : IHostedService
{
	private readonly IServiceProvider _serviceProvider;
	private Timer _timer;
	public NotificationSender(IServiceProvider provider)
	{
		_serviceProvider = provider;
	}

	private async void SendNotificationAsync(object data)
	{
		var notificationMessage = (NotificationMessage)data;
		var notificationService = _serviceProvider.GetRequiredKeyedService<INotificationService>(notificationMessage.Type);
		await notificationService.SendAsync(notificationMessage.Recipient, notificationMessage.Subject, notificationMessage.Content);

		// Mesaj tipine göre uygun metodu çağır
		//switch (notificationMessage.ToType)
		//{
		//	case NotificationToTypes.ToAll:
		//		await notificationService.SendNotificationToAllAsync(notificationMessage.Content);
		//		break;
		//	case NotificationToTypes.ToUser:
		//		await notificationService.SendNotificationToUserAsync(notificationMessage.Recipient, notificationMessage.Content);
		//		break;
		//	case NotificationToTypes.ToCompanyAdmins:
		//		await notificationService.SendNotificationToCompanyAdminsAsync(notificationMessage.Content);
		//		break;
		//	case NotificationToTypes.ToCompany:
		//		if (Guid.TryParse(notificationMessage.Recipient, out Guid companyId))
		//		{
		//			await notificationService.SendNotificationToCompanyAsync(companyId, notificationMessage.Content);
		//		}
		//		break;
		//	default:
		//		// Eski yöntem için uyumluluk (bu kısım kaldırılabilir)
		//		Console.WriteLine($"Sending notification to {notificationMessage.Recipient}: {notificationMessage.Content}");
		//		break;
		//}
	}

	public Task StartAsync(NotificationMessage notificationMessage)
	{
		_timer = new Timer(SendNotificationAsync, notificationMessage, TimeSpan.Zero, TimeSpan.Zero);
		return Task.CompletedTask;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
