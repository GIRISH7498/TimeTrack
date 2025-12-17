using TimeTrack.Domain.Enums;

namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class PushDevice : EntityBase
    {
        public long DeviceId { get; set; }
        public int UserId { get; set; }
        public PushPlatform Platform { get; set; }
        public string PushToken { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTime? LastSeenAt { get; set; }
    }
}
