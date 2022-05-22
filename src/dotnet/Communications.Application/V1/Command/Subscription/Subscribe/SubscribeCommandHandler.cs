using System.Text.RegularExpressions;
using Communications.Application.Common.Errors;
using Communications.Application.Common.Models;
using Communications.Domain.Models;
using Communications.SubscriptionService.Interfaces;
using Communications.SubscriptionService.Requests;
using MediatR;

namespace Communications.Application.V1.Command.Subscription.Subscribe
{
	public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, CommandResponse<BunSubscription>>
	{
		private readonly IMediator _mediator;
		private readonly ISubscribe _subscribe;

		public SubscribeCommandHandler(IMediator mediator, ISubscribe subscribe)
		{
			_mediator = mediator;
			_subscribe = subscribe;
		}

		public async Task<CommandResponse<BunSubscription>> Handle(SubscribeCommand command,
			CancellationToken cancellationToken)
		{
			var result = new CommandResponse<BunSubscription>
			{
				Errors = new List<Error>(),
			};

			if (string.IsNullOrEmpty(command.EmailAddress))
			{
				result.Errors.Add(new RequiredFieldMissingError("EmailAddress", "Email Address cannot be empty."));
			}

			var regex = new Regex(
				"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
			if (!string.IsNullOrEmpty(command.EmailAddress) && !regex.IsMatch(command.EmailAddress))
			{
				result.Errors.Add(new InvalidEmailAddressError("EmailAddress", command.EmailAddress));
			}


			var subscribeResult =
				await _subscribe.SubscribeAsync(new SubscribeRequest() { EmailAddress = command.EmailAddress });

			return result;
		}
	}
}