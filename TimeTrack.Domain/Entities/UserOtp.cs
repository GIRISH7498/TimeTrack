namespace TimeTrack.Domain.Entities
{
    public class UserOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Code { get; set; } = default!;
        public string Purpose { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAtUtc { get; set; }
        public int AttemptCount { get; set; }
    }
}
