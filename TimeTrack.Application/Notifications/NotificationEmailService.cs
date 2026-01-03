using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities.NotificationModels;
using TimeTrack.Domain.Enums;

namespace TimeTrack.Application.Notifications
{
    public class NotificationEmailService : INotificationEmailService
    {
        private readonly IApplicationDbContext _context;

        public NotificationEmailService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<long> EnqueueEmailAsync(
            string templateKey,
            int categoryId,
            int userId,
            string email,
            IDictionary<string, object?> templateData,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new NotFoundException("Email not found.", nameof(email));

            // 1) Resolve template for Email channel
            var template = await _context.NotificationTemplates
                .AsNoTracking()
                .Where(t =>
                    t.ChannelId == NotificationChannelType.Email &&
                    t.TemplateKey == templateKey &&
                    t.IsActive)
                .OrderByDescending(t => t.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (template is null)
            {
                throw new NotFoundException(
                    $"No active email template found for key '{templateKey}'.");
            }

            var nowUtc = DateTime.UtcNow;

            // 2) Create NotificationEvent
            var notificationEvent = new NotificationEvent
            {
                CategoryId = categoryId,
                TemplateKey = templateKey,
                TemplateDataJson = JsonSerializer.Serialize(templateData ?? new Dictionary<string, object?>()),
                Priority = 0,
                CreatedBy = userId.ToString(),
                IdempotencyKey = null
            };

            // 3) Recipient
            var recipient = new NotificationRecipient
            {
                Notification = notificationEvent,
                UserId = userId,
                Email = email,
                TargetKey = $"E:{email.ToLowerInvariant()}"
            };

            notificationEvent.Recipients.Add(recipient);

            // 4) Message (Email channel)
            var message = new NotificationMessage
            {
                Notification = notificationEvent,
                Recipient = recipient,
                ChannelId = NotificationChannelType.Email,
                Status = NotificationMessageStatus.Pending,
                ScheduledAt = nowUtc,
                TemplateId = template.TemplateId
            };

            notificationEvent.Messages.Add(message);
            recipient.Messages.Add(message);

            // 5) Persist
            _context.NotificationEvents.Add(notificationEvent);
            _context.NotificationRecipients.Add(recipient);
            _context.NotificationMessages.Add(message);

            await _context.SaveChangesAsync(cancellationToken);

            return message.MessageId;
        }
    }
}
