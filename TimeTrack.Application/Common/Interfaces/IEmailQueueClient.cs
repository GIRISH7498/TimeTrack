namespace TimeTrack.Application.Common.Interfaces
{
    public interface IEmailQueueClient
    {
        Task EnqueueAsync(long notificationMessageId, CancellationToken cancellationToken);
    }
}
