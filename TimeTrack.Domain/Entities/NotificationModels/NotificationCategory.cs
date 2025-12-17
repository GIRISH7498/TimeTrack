namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationCategory : EntityBase
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public bool AllowUnsubscribe { get; set; }

        public ICollection<NotificationTemplate> Templates { get; set; } = new List<NotificationTemplate>();
        public ICollection<NotificationPreference> Preferences { get; set; } = new List<NotificationPreference>();
    }
}
