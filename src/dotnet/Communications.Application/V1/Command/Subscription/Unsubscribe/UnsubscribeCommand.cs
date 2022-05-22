using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.Application.Common.Models;
using Communications.Domain.Models;
using MediatR;

namespace Communications.Application.V1.Command.Subscription.Unsubscribe
{
	public class UnsubscribeCommand : IRequest<CommandResponse<BunSubscription>>
	{
		public string EmailAddress { get; set; }
	}
}
