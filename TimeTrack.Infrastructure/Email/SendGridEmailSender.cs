using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TimeTrack.Application.Common.Email;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.Infrastructure.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(
            IOptions<SendGridOptions> options,
            ILogger<SendGridEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailSendRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("SendGrid API key is not configured.");
            }

            var client = new SendGridClient(_options.ApiKey);

            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(request.ToEmail, request.ToName);

            var msg = new SendGridMessage
            {
                From = from,
                Subject = request.Subject,
                HtmlContent = request.HtmlBody,
                PlainTextContent = request.TextBody
            };

            msg.AddTo(to);

            // Attachments
            foreach (var att in request.Attachments)
            {
                var base64 = Convert.ToBase64String(att.Content);
                msg.AddAttachment(att.FileName, base64, att.ContentType, att.IsInline ? "inline" : "attachment", att.ContentId);
            }

            _logger.LogInformation("Sending email to {Email}", request.ToEmail);

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                _logger.LogInformation("Email sent successfully to {Email}", request.ToEmail);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Body: {Body}",
                    request.ToEmail,
                    response.StatusCode,
                    body);

                throw new InvalidOperationException(
                    $"SendGrid send failed with status {response.StatusCode}: {body}");
            }
        }
    }
}
