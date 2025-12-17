using TimeTrack.Domain.Enums;

namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationPreference : EntityBase
    {
        public long PreferenceId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public NotificationChannelType ChannelId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? QuietHoursJson { get; set; }
        public NotificationCategory Category { get; set; } = default!;
    }
}
