using MediatR;
using Microsoft.Extensions.Logging;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehaviour(
            ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var email = _currentUserService.Email;

            _logger.LogInformation(
                "Handling {RequestName} for UserId {UserId}, Email {Email}",
                requestName,
                userId,
                email);

            var response = await next();

            _logger.LogInformation(
                "Handled {RequestName} for UserId {UserId}",
                requestName,
                userId);

            return response;
        }
    }
}
