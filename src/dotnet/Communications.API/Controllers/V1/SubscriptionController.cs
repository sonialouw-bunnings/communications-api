using Communications.Api.Contracts.V1.Requests;
using Communications.Api.Contracts.V1.Responses;
using Communications.Api.Routes.V1;
using Communications.API.Controllers;
using Communications.Application.V1.Command.Subscription.Subscribe;
using Communications.Application.V1.Command.Subscription.Unsubscribe;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Communications.Api.Controllers.V1
{
	[Produces("application/json")]
	[ApiController]
	[ApiVersion("1.0")]
	public class SubscriptionController : BaseController
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		[Route(ApiRoutes.Subscription.Subscribe)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[SwaggerOperation(
			Summary = "Subscribe a customer",
			Description = "Subscribe a customer",
			Tags = new[] { "Subscribe" }
		)]
		//[Authorize(policy: CanSubscribe.PrivilegeName)]
		public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
		{
			var command = new SubscribeCommand()
			{
				EmailAddress = request.EmailAddress
			};

			await Mediator.Send(command);

			return Created(ApiRoutes.Subscription.Subscribe, new UnsubscribeResponse() { Success = true });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		[Route(ApiRoutes.Subscription.Unsubscribe)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[SwaggerOperation(
			Summary = "Unsubscribe a customer",
			Description = "Unsubscribe a customer",
			Tags = new[] { "Unsubscribe" }
		)]
		//[Authorize(policy: CanUnsubscribe.PrivilegeName)]
		public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
		{
			var command = new UnsubscribeCommand()
			{
				EmailAddress = request.EmailAddress
			};

			await Mediator.Send(command);

			return Created(ApiRoutes.Subscription.Unsubscribe, new UnsubscribeResponse() { Success = true });
		}
	}
}