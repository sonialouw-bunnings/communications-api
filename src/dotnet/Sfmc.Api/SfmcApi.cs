using Communications.SubscriptionService.Interfaces;
using Communications.SubscriptionService.Requests;
using Communications.SubscriptionService.Results;

namespace Sfmc.Api
{
	public class SfmcApi : IUnsubscribe, ISubscribe, IGetSubscription
	{
		public Task<SubscribeResult> SubscribeAsync(SubscribeRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<UnsubscribeResult> UnsubscribeAsync(SubscribeRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<GetSubscriptionResult> GetSubsciption(GetSubscriptionRequest request)
		{
			throw new NotImplementedException();
		}
	}
}