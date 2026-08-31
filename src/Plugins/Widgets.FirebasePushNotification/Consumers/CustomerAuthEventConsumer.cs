using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;

namespace Widgets.FirebasePushNotification.Consumers;

public class CustomerAuthEventConsumer : IConsumer<CustomerRegisteredEvent>, IConsumer<CustomerLoggedinEvent>
{
	public Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
	{
		return Task.CompletedTask;
	}

	public Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
	{
		return Task.CompletedTask;
	}
}
