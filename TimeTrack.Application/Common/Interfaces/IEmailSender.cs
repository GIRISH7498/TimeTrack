using TimeTrack.Application.Common.Email;

namespace TimeTrack.Application.Common.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(EmailSendRequest request, CancellationToken cancellationToken);
    }
}
