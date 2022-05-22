﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.SubscriptionService.Requests;
using Communications.SubscriptionService.Results;

namespace Communications.SubscriptionService.Interfaces
{
	public interface IGetSubscription
	{
		Task<GetSubscriptionResult> GetSubsciption(GetSubscriptionRequest request);
	}
}