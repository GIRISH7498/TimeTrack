namespace TimeTrack.Application.Common.Interfaces
{
    public interface INotificationEmailService
    {
        /// <summary>
        /// Enqueues an email notification using your Notification* tables.
        /// Creates NotificationEvent, NotificationRecipient, NotificationMessage
        /// with Channel = Email.
        /// Returns the NotificationMessageId.
        /// </summary>
        Task<long> EnqueueEmailAsync(
            string templateKey,
            int categoryId,
            int userId,
            string email,
            IDictionary<string, object?> templateData,
            CancellationToken cancellationToken);
    }
}
