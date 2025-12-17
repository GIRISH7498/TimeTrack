namespace TimeTrack.Application.Common.Interfaces
{
    public interface IBellNotificationService
    {
        /// <summary>
        /// Creates a bell notification for a single user, persists it to the notification tables
        /// and triggers real-time delivery (via SSE) if the user is connected.
        /// Returns the BellInboxItem primary key (InboxId).
        /// </summary>
        Task<long> CreateAsync(
            int userId,
            string title,
            string body,
            string? deepLinkUrl,
            CancellationToken cancellationToken);
    }
}
