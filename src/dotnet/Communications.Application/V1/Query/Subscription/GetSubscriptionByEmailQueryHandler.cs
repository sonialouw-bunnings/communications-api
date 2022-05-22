using Communications.Domain.Models;
using MediatR;

namespace Communications.Application.V1.Query.Subscribe
{
	public class GetSubscriptionByEmailQueryHandler : IRequestHandler<GetSubscriptionByEmailQuery, BunSubscription>
	{
		public GetSubscriptionByEmailQueryHandler()
		{
		}

		public async Task<BunSubscription> Handle(GetSubscriptionByEmailQuery request, CancellationToken cancellationToken)
		{
			// call SFMC
			return new BunSubscription();
		}
	}
}
