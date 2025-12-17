namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationOutbox : EntityBase
    {
        public long OutboxId { get; set; }
        public string EventType { get; set; } = default!;
        public string AggregateId { get; set; } = default!;
        public string IdempotencyKey { get; set; } = default!;
        public string PayloadJson { get; set; } = default!;
        public DateTime? ProcessedAt { get; set; }
    }
}
