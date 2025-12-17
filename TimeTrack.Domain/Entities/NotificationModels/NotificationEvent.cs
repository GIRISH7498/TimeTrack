namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationEvent : EntityBase
    {
        public long NotificationId { get; set; }
        public int CategoryId { get; set; }
        public string TemplateKey { get; set; } = default!;
        public string TemplateDataJson { get; set; } = "{}";
        public byte Priority { get; set; } = 0;
        public string? CreatedBy { get; set; }
        public string? IdempotencyKey { get; set; }
        public NotificationCategory Category { get; set; } = default!;
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
        public ICollection<NotificationMessage> Messages { get; set; } = new List<NotificationMessage>();
    }
}
