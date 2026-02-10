using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TimeTrack.Application.Common.Email;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Enums;
using TimeTrack.Infrastructure.Email;

namespace TimeTrack.API.Services.Notifications
{
    public class EmailQueueProcessor : IHostedService, IAsyncDisposable
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailQueueProcessor> _logger;

        public EmailQueueProcessor(
            IOptions<EmailQueueOptions> options,
            IServiceProvider serviceProvider,
            ILogger<EmailQueueProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var cfg = options.Value;

            if (string.IsNullOrWhiteSpace(cfg.ConnectionString))
                throw new InvalidOperationException("EmailQueue:ConnectionString is not configured.");

            if (string.IsNullOrWhiteSpace(cfg.QueueName))
                throw new InvalidOperationException("EmailQueue:QueueName is not configured.");

            var client = new ServiceBusClient(cfg.ConnectionString);
            _processor = client.CreateProcessor(cfg.QueueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            _logger.LogInformation("Starting EmailQueueProcessor ServiceBusProcessor.");
            await _processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping EmailQueueProcessor ServiceBusProcessor.");

            await _processor.StopProcessingAsync(cancellationToken);

            _processor.ProcessMessageAsync -= ProcessMessageAsync;
            _processor.ProcessErrorAsync -= ProcessErrorAsync;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var templateRenderer = scope.ServiceProvider.GetRequiredService<IEmailTemplateRenderer>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailQueueProcessor>>();

            try
            {
                var bodyString = args.Message.Body.ToString();

                var payload = JsonSerializer.Deserialize<EmailNotificationPayload>(
                    bodyString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (payload is null || payload.NotificationMessageId == 0)
                {
                    logger.LogWarning("Invalid email queue payload: {Body}", bodyString);
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                var messageId = payload.NotificationMessageId;

                // 1) Lightweight projection: only fetch what we need to send the email
                var job = await context.NotificationMessages
                    .Where(m => m.MessageId == messageId)
                    .Select(m => new EmailJobData
                    {
                        MessageId = m.MessageId,
                        RecipientEmail = m.Recipient.Email,
                        TemplateSubject = m.Template!.Subject,
                        TemplateBody = m.Template.Body,
                        TemplateDataJson = m.Notification.TemplateDataJson,
                        Attachments = m.Attachments.Select(a => new EmailJobAttachment
                        {
                            FileName = a.FileName,
                            ContentType = a.ContentType,
                            Content = a.Content,
                            IsInline = a.IsInline,
                            ContentId = a.ContentId
                        }).ToList()
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(args.CancellationToken);

                if (job is null)
                {
                    logger.LogWarning("NotificationMessageId {MessageId} not found in database.", messageId);
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                if (string.IsNullOrWhiteSpace(job.RecipientEmail))
                {
                    logger.LogWarning("NotificationMessageId {MessageId} has null recipient email.", messageId);
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                // 2) Render template
                var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    job.TemplateDataJson ?? "{}")
                    ?? new Dictionary<string, object?>();

                var rendered = await templateRenderer.RenderAsync(
                    job.TemplateSubject,
                    job.TemplateBody,
                    data,
                    args.CancellationToken);

                var emailRequest = new EmailSendRequest
                {
                    ToEmail = job.RecipientEmail,
                    ToName = null,
                    Subject = rendered.Subject,
                    HtmlBody = rendered.HtmlBody
                };

                foreach (var att in job.Attachments)
                {
                    emailRequest.Attachments.Add(new EmailAttachment
                    {
                        FileName = att.FileName,
                        ContentType = att.ContentType,
                        Content = att.Content,
                        IsInline = att.IsInline,
                        ContentId = att.ContentId
                    });
                }

                // 3) Send email
                await emailSender.SendAsync(emailRequest, args.CancellationToken);

                // 4) Update status in DB (load only the message entity)
                var msgEntity = await context.NotificationMessages
                    .FirstOrDefaultAsync(m => m.MessageId == job.MessageId, args.CancellationToken);

                if (msgEntity != null)
                {
                    msgEntity.Status = NotificationMessageStatus.Sent;
                    msgEntity.SentAt = DateTime.UtcNow;
                    msgEntity.LastError = null;

                    await context.SaveChangesAsync(args.CancellationToken);
                }

                // 5) Complete queue message
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing email queue message. ServiceBusMessageId: {MessageId}",
                    args.Message.MessageId);

                // Optional: you could dead-letter here instead of complete,
                // but be careful to avoid infinite poison-message loops.
                await args.CompleteMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception,
                "ServiceBus error in EmailQueueProcessor. Entity: {EntityPath}, ErrorSource: {ErrorSource}",
                args.EntityPath,
                args.ErrorSource);

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.DisposeAsync();
        }

        private sealed class EmailNotificationPayload
        {
            public long NotificationMessageId { get; set; }
        }

        private sealed class EmailJobData
        {
            public long MessageId { get; set; }
            public string? RecipientEmail { get; set; }
            public string? TemplateSubject { get; set; }
            public string TemplateBody { get; set; } = default!;
            public string? TemplateDataJson { get; set; }
            public List<EmailJobAttachment> Attachments { get; set; } = new();
        }

        private sealed class EmailJobAttachment
        {
            public string FileName { get; set; } = default!;
            public string ContentType { get; set; } = default!;
            public byte[] Content { get; set; } = Array.Empty<byte>();
            public bool IsInline { get; set; }
            public string? ContentId { get; set; }
        }

    }
}
