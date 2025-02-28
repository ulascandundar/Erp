using Erp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Models;

public class NotificationMessage
{
	public NotificationTypes Type { get; set; }
	public string Recipient { get; set; }
	public string Subject { get; set; }
	public string Content { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}