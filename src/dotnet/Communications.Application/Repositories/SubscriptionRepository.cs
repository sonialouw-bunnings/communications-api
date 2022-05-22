using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.Domain.Models;
using Communications.Domain.Repositories;

namespace Communications.Application.Repositories
{
	public class SubscriptionRepository : ISubscriptionRepository

	{
		public Task<BunSubscription> GetSubscription(Guid id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
