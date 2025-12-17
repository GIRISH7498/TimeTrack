namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationUnsubscribe : EntityBase
    {
        public long UnsubscribeId { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneE164 { get; set; }
        public int CategoryId { get; set; }
        public DateTime UnsubscribedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
    }
}
