using Erp.Domain.Interfaces.BusinessServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Notifications.Services;

public class EmailNotification : INotificationService
{
	public async Task SendAsync(string recipient, string subject, string content)
	{
		Console.WriteLine($"Preparing to send Email to {recipient}");

		// Simulate processing time
		await Task.Delay(3000);

		// TODO: Implement actual Email sending logic here

		Console.WriteLine($"Email sent to {recipient}: {content}");
	}
}
