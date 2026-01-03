namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationAttachment : EntityBase
    {
        public long AttachmentId { get; set; }
        public long MessageId { get; set; } 
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public bool IsInline { get; set; } = false;
        public string? ContentId { get; set; }
        public NotificationMessage Message { get; set; } = default!;
    }
}
