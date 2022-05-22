using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.Domain.Models;

namespace Communications.Domain.Repositories
{
	public interface ISubscriptionRepository
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<BunSubscription> GetSubscription(Guid id, CancellationToken cancellationToken);
	}
}
