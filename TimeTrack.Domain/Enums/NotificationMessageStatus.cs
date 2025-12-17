namespace TimeTrack.Domain.Enums
{
    public enum NotificationMessageStatus : byte
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3,
        Cancelled = 4,
        DeadLetter = 5
    }
}
