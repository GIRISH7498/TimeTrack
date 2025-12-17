using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities.NotificationModels;
using TimeTrack.Domain.Enums;

namespace TimeTrack.Infrastructure.Services
{
    public class BellNotificationService : IBellNotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationDispatcher _dispatcher;
        private readonly ILogger<BellNotificationService> _logger;

        public BellNotificationService(
            IApplicationDbContext context,
            INotificationDispatcher dispatcher,
            ILogger<BellNotificationService> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task<long> CreateAsync(
            int userId,
            string title,
            string body,
            string? deepLinkUrl,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            // 1) Create NotificationEvent (simple generic bell event)
            var notificationEvent = new NotificationEvent
            {
                CategoryId = 0, // you can use real category IDs later
                TemplateKey = "Bell.Generic",
                TemplateDataJson = JsonSerializer.Serialize(new
                {
                    title,
                    body,
                    deepLinkUrl
                }),
                Priority = 0,
                CreatedBy = userId.ToString()
            };

            // 2) Recipient (target = this user)
            var recipient = new NotificationRecipient
            {
                Notification = notificationEvent,
                UserId = userId,
                TargetKey = $"U:{userId}"
            };

            notificationEvent.Recipients.Add(recipient);

            // 3) NotificationMessage for Bell channel
            var message = new NotificationMessage
            {
                Notification = notificationEvent,
                Recipient = recipient,
                ChannelId = NotificationChannelType.Bell,
                Status = NotificationMessageStatus.Sent,  // treat as sent once persisted
                ScheduledAt = now,
                SentAt = now
            };

            notificationEvent.Messages.Add(message);
            recipient.Messages.Add(message);

            // 4) BellInboxItem row (used by bell dropdown + SSE)
            var inboxItem = new BellInboxItem
            {
                Message = message,
                UserId = userId,
                Title = title,
                Body = body,
                DeepLinkUrl = deepLinkUrl,
                IsRead = false
                // CreatedAt comes from EntityBase default (UtcNow)
            };

            message.BellInboxItem = inboxItem;

            // 5) Persist everything
            _context.NotificationEvents.Add(notificationEvent);
            _context.NotificationRecipients.Add(recipient);
            _context.NotificationMessages.Add(message);
            _context.BellInboxItems.Add(inboxItem);

            await _context.SaveChangesAsync(cancellationToken);

            // 6) Real-time push via dispatcher (SSE in API layer)
            try
            {
                await _dispatcher.DispatchBellNotificationAsync(userId, inboxItem, cancellationToken);
            }
            catch (Exception ex)
            {
                // SSE is best-effort; DB is already saved so user will still see it later
                _logger.LogError(ex,
                    "Failed to dispatch bell notification via SSE for user {UserId}, InboxId {InboxId}",
                    userId,
                    inboxItem.InboxId);
            }

            return inboxItem.InboxId;
        }
    }
}
