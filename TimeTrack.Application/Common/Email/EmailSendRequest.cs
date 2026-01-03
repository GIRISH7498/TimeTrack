namespace TimeTrack.Application.Common.Email
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public bool IsInline { get; set; }
        public string? ContentId { get; set; } 
    }

    public class EmailSendRequest
    {
        public string ToEmail { get; set; } = default!;
        public string? ToName { get; set; }
        public string Subject { get; set; } = default!;
        public string HtmlBody { get; set; } = default!;
        public string? TextBody { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new();
    }
}
