using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TimeTrack.API.Services.Notifications
{
    // Represents a single SSE client connection for a user
    public class SseClient
    {
        public int UserId { get; }
        public Channel<string> MessageChannel { get; }

        public SseClient(int userId)
        {
            UserId = userId;
            MessageChannel = Channel.CreateUnbounded<string>();
        }
    }

    public interface ISseConnectionManager
    {
        SseClient AddClient(int userId);
        void RemoveClient(int userId, SseClient client);
        Task SendToUserAsync(int userId, string message, CancellationToken cancellationToken);
    }

    public class SseConnectionManager : ISseConnectionManager
    {
        private readonly ConcurrentDictionary<int, List<SseClient>> _clients = new();

        public SseClient AddClient(int userId)
        {
            var client = new SseClient(userId);

            var list = _clients.GetOrAdd(userId, _ => new List<SseClient>());
            lock (list)
            {
                list.Add(client);
            }

            return client;
        }

        public void RemoveClient(int userId, SseClient client)
        {
            if (_clients.TryGetValue(userId, out var list))
            {
                lock (list)
                {
                    list.Remove(client);
                    if (list.Count == 0)
                    {
                        _clients.TryRemove(userId, out _);
                    }
                }
            }
        }

        public async Task SendToUserAsync(int userId, string message, CancellationToken cancellationToken)
        {
            if (!_clients.TryGetValue(userId, out var list))
            {
                return; // user has no active SSE connections
            }

            List<SseClient> snapshot;
            lock (list)
            {
                snapshot = list.ToList();
            }

            foreach (var client in snapshot)
            {
                await client.MessageChannel.Writer.WriteAsync(message, cancellationToken);
            }
        }
    }
}
