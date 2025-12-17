namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationRecipient : EntityBase
    {
        public long RecipientId { get; set; }
        public long NotificationId { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneE164 { get; set; }
        public string TargetKey { get; set; } = default!;
        public NotificationEvent Notification { get; set; } = default!;
        public ICollection<NotificationMessage> Messages { get; set; } = new List<NotificationMessage>();
    }
}
