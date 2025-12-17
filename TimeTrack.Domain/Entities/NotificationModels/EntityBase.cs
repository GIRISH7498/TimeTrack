namespace TimeTrack.Domain.Entities.NotificationModels
{
    public abstract class EntityBase
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // optimistic concurrency (SQL Server rowversion)
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
