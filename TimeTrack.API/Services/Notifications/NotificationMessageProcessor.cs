using Microsoft.EntityFrameworkCore;
using Serilog;
using TimeTrack.Application.Common.Email;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Enums;

namespace TimeTrack.API.Services.Notifications
{
    public class NotificationMessageProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

        public NotificationMessageProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    var templateRenderer = scope.ServiceProvider.GetRequiredService<IEmailTemplateRenderer>();

                    var now = DateTime.UtcNow;

                    var pendingEmails = await context.NotificationMessages
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(m => m.Recipient)
                        .Include(m => m.Notification)
                        .Include(m => m.Template)
                        .Include(m => m.Attachments)
                        .Where(m =>
                            m.ChannelId == NotificationChannelType.Email &&
                            m.Status == NotificationMessageStatus.Pending &&
                            m.ScheduledAt <= now &&
                            (m.NextRetryAt == null || m.NextRetryAt <= now))
                        .OrderBy(m => m.ScheduledAt)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    if (!pendingEmails.Any())
                    {
                        await Task.Delay(_pollInterval, stoppingToken);
                        continue;
                    }

                    foreach (var msg in pendingEmails)
                    {
                        try
                        {
                            msg.Status = NotificationMessageStatus.Processing;
                            msg.AttemptCount += 1;

                            var recipient = msg.Recipient;
                            var notification = msg.Notification;
                            var template = msg.Template;

                            if (recipient.Email is null)
                            {
                                msg.Status = NotificationMessageStatus.Failed;
                                msg.LastError = "Recipient email is null.";
                                continue;
                            }

                            if (template is null)
                            {
                                msg.Status = NotificationMessageStatus.Failed;
                                msg.LastError = "Email template not resolved.";
                                continue;
                            }

                            // Prepare data for template rendering
                            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(
                                notification.TemplateDataJson ?? "{}")
                                ?? new Dictionary<string, object?>();

                            var rendered = await templateRenderer.RenderAsync(
                                template.Subject,
                                template.Body,
                                data,
                                stoppingToken);

                            var emailRequest = new EmailSendRequest
                            {
                                ToEmail = recipient.Email,
                                ToName = null,
                                Subject = rendered.Subject,
                                HtmlBody = rendered.HtmlBody
                            };

                            // Attachments
                            foreach (var att in msg.Attachments)
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

                            await emailSender.SendAsync(emailRequest, stoppingToken);

                            msg.Status = NotificationMessageStatus.Sent;
                            msg.SentAt = DateTime.UtcNow;
                            msg.LastError = null;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to process email NotificationMessageId {MessageId}", msg.MessageId);

                            msg.Status = NotificationMessageStatus.Failed;
                            msg.LastError = ex.Message;
                            msg.NextRetryAt = DateTime.UtcNow.AddMinutes(5);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "NotificationMessageProcessor loop error");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }
}
