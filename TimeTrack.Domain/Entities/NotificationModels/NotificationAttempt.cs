using TimeTrack.Domain.Enums;

namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationAttempt : EntityBase
    {
        public long AttemptId { get; set; }
        public long MessageId { get; set; }
        public int AttemptNo { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public AttemptResultStatus ResultStatus { get; set; }
        public string Provider { get; set; } = default!;
        public string? ProviderResponseJson { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public NotificationMessage Message { get; set; } = default!;
    }
}
