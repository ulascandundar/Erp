using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Notifications.Sender;
using Erp.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Notifications.Configurations;

public static class NotificationFactoryRegister
{
	public static void RegisterNotificationFactory(this IServiceCollection services)
	{
		//services.AddScoped<INotificationSender, NotificationSender>();
		services.AddSingleton<NotificationSender>();
		services.AddHostedService<NotificationSender>();
		services.AddKeyedSingleton<INotificationService, EmailNotification>(NotificationTypes.Email);
		services.AddKeyedSingleton<INotificationService, SmsNotification>(NotificationTypes.Sms);
		services.AddKeyedSingleton<INotificationService, WhatsAppNotification>(NotificationTypes.WhatsApp);
		services.AddKeyedSingleton<INotificationService, FireBaseNotification>(NotificationTypes.Firebase);
	}
}
