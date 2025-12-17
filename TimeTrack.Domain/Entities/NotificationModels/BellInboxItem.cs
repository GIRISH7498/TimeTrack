namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class BellInboxItem : EntityBase
    {
        public long InboxId { get; set; }
        public long MessageId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string? DeepLinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationMessage Message { get; set; } = default!;
    }
}
