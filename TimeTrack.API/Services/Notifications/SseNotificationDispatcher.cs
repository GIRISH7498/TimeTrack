using System.Text.Json;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities.NotificationModels;

namespace TimeTrack.API.Services.Notifications
{
    public class SseNotificationDispatcher : INotificationDispatcher
    {
        private readonly ISseConnectionManager _connectionManager;

        public SseNotificationDispatcher(ISseConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task DispatchBellNotificationAsync(
            int userId,
            BellInboxItem inboxItem,
            CancellationToken cancellationToken)
        {
            var payload = new
            {
                inboxId = inboxItem.InboxId,
                title = inboxItem.Title,
                body = inboxItem.Body,
                deepLinkUrl = inboxItem.DeepLinkUrl,
                isRead = inboxItem.IsRead,
                createdAt = inboxItem.CreatedAt,
                readAt = inboxItem.ReadAt
            };

            var json = JsonSerializer.Serialize(payload);

            await _connectionManager.SendToUserAsync(userId, json, cancellationToken);
        }
    }
}
