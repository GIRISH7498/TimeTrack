using TimeTrack.Domain.Enums;

namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationTemplate : EntityBase
    {
        public long TemplateId { get; set; }
        public NotificationChannelType ChannelId { get; set; }
        public int CategoryId { get; set; }
        public string TemplateKey { get; set; } = default!;
        public int Version { get; set; }
        public string? LanguageCode { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public NotificationCategory Category { get; set; } = default!;
    }
}
