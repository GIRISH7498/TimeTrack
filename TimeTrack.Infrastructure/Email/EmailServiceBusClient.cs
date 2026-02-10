using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.Infrastructure.Email
{
    public class EmailServiceBusClient : IEmailQueueClient, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<EmailServiceBusClient> _logger;

        public EmailServiceBusClient(
            IOptions<EmailQueueOptions> options,
            ILogger<EmailServiceBusClient> logger)
        {
            _logger = logger;
            var config = options.Value;

            if (string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new InvalidOperationException("EmailQueue:ConnectionString is not configured.");

            if (string.IsNullOrWhiteSpace(config.QueueName))
                throw new InvalidOperationException("EmailQueue:QueueName is not configured.");

            _client = new ServiceBusClient(config.ConnectionString);
            _sender = _client.CreateSender(config.QueueName);
        }

        public async Task EnqueueAsync(long notificationMessageId, CancellationToken cancellationToken)
        {
            var payload = new { notificationMessageId };

            var body = JsonSerializer.Serialize(payload);
            var message = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                Subject = "EmailNotification"
            };

            _logger.LogInformation(
                "Enqueueing email notification for NotificationMessageId {MessageId}",
                notificationMessageId);

            await _sender.SendMessageAsync(message, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
