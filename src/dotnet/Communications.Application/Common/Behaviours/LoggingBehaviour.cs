using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Communications.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger _logger;

        public LoggingBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Communications Request: {Name} {@Request}", requestName, request);

            return Task.CompletedTask;
        }
    }
}