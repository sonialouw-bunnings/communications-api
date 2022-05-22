using Communications.Domain.Models;
using MediatR;

namespace Communications.Application.V1.Query.Subscribe
{
	public class GetSubscriptionByEmailQuery : IRequest<BunSubscription>
	{
		public string EmailAddress { get; set; }
	}
}