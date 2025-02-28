using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface INotificationService
{
	Task SendAsync(string recipient, string subject, string content);
}
