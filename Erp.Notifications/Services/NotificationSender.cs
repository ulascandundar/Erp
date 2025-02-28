using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Notifications.Services;

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
