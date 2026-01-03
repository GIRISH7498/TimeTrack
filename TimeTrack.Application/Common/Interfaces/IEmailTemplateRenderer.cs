namespace TimeTrack.Application.Common.Interfaces
{
    public class RenderedEmailTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }

    public interface IEmailTemplateRenderer
    {
        /// <summary>
        /// Renders subject and body using the given template strings and data.
        /// </summary>
        Task<RenderedEmailTemplate> RenderAsync(
            string? subjectTemplate,
            string bodyTemplate,
            IDictionary<string, object?> data,
            CancellationToken cancellationToken);
    }
}
