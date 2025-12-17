using TimeTrack.Domain.Entities.NotificationModels;

namespace TimeTrack.Application.Common.Interfaces
{
    public interface INotificationDispatcher
    {
        /// <summary>
        /// Dispatches a bell notification (BellInboxItem) for the given user
        /// to any connected real-time channels (SSE, etc.).
        /// </summary>
        Task DispatchBellNotificationAsync(
            int userId,
            BellInboxItem inboxItem,
            CancellationToken cancellationToken);
    }
}
