using TimeTrack.Domain.Enums;

namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationMessage : EntityBase
    {
        public long MessageId { get; set; }
        public long NotificationId { get; set; }
        public long RecipientId { get; set; }
        public NotificationChannelType ChannelId { get; set; }
        public long? TemplateId { get; set; }
        public NotificationMessageStatus Status { get; set; } = NotificationMessageStatus.Pending;
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        public DateTime? LockedUntil { get; set; }
        public Guid? LockId { get; set; }
        public int AttemptCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? LastError { get; set; }
        public DateTime? SentAt { get; set; }
        public NotificationEvent Notification { get; set; } = default!;
        public NotificationRecipient Recipient { get; set; } = default!;
        public NotificationTemplate? Template { get; set; }
        public ICollection<NotificationAttempt> Attempts { get; set; } = new List<NotificationAttempt>();
        public BellInboxItem? BellInboxItem { get; set; }
    }
}
