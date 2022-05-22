using Communications.Application.Common.Models;
using Communications.Domain.Models;
using Communications.SubscriptionService.Interfaces;
using Communications.SubscriptionService.Requests;
using MediatR;

namespace Communications.Application.V1.Command.Subscription.Unsubscribe
{
	public class SubscribeCommandHandler : IRequestHandler<UnsubscribeCommand, CommandResponse<BunSubscription>>
	{
		private readonly IMediator _mediator;
		private readonly IUnsubscribe _unSubscribe;

		public SubscribeCommandHandler(IMediator mediator, IUnsubscribe unSubscribe)
		{
			_mediator = mediator;
			_unSubscribe = unSubscribe;
		}

		public async Task<CommandResponse<BunSubscription>> Handle(UnsubscribeCommand command,
			CancellationToken cancellationToken)
		{
			var result = new CommandResponse<BunSubscription>
			{
				Errors = new List<Error>(),
			};


			await _unSubscribe.UnsubscribeAsync(new SubscribeRequest() { EmailAddress = command.EmailAddress });

			return result;
		}
	}
}