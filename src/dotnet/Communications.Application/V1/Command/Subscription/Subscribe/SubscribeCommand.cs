using Communications.Application.Common.Models;
using Communications.Domain.Models;
using MediatR;

namespace Communications.Application.V1.Command.Subscription.Subscribe
{
	public class SubscribeCommand : IRequest<CommandResponse<BunSubscription>>
	{
		public string EmailAddress { get; set; }
	}
}
