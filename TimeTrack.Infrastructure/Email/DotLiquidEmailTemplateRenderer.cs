using DotLiquid;
using System.Text.Json;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.Infrastructure.Email
{
    public class DotLiquidEmailTemplateRenderer : IEmailTemplateRenderer
    {
        // Optional: ensure C#-style names like "firstName" work out of the box
        static DotLiquidEmailTemplateRenderer()
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
        }

        public Task<RenderedEmailTemplate> RenderAsync(
            string? subjectTemplate,
            string bodyTemplate,
            IDictionary<string, object?> data,
            CancellationToken cancellationToken)
        {
            if (bodyTemplate is null)
            {
                throw new NotFoundException("Body template cannot be null.");
            }

            var normalized = new Dictionary<string, object?>();

            foreach (var kvp in data ?? new Dictionary<string, object?>())
            {
                normalized[kvp.Key] = NormalizeValue(kvp.Value);
            }

            var hash = Hash.FromDictionary(normalized);
            
            // Subject
            var renderedSubject = string.Empty;
            if (!string.IsNullOrWhiteSpace(subjectTemplate))
            {
                var subjectTpl = Template.Parse(subjectTemplate);
                renderedSubject = subjectTpl.Render(hash);
            }

            // Body
            var bodyTpl = Template.Parse(bodyTemplate);
            var renderedBody = bodyTpl.Render(hash);

            var result = new RenderedEmailTemplate
            {
                Subject = renderedSubject,
                HtmlBody = renderedBody
            };

            return Task.FromResult(result);
        }

        private static object? NormalizeValue(object? value)
        {
            if (value is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:
                        return je.GetString();

                    case JsonValueKind.Number:
                        if (je.TryGetInt64(out var l)) return l;
                        if (je.TryGetDouble(out var d)) return d;
                        return je.ToString();

                    case JsonValueKind.True:
                        return true;

                    case JsonValueKind.False:
                        return false;

                    case JsonValueKind.Null:
                        return null;

                    default:
                        // For objects/arrays, you can either ToString() or ignore;
                        // for templates we usually just need strings anyway.
                        return je.ToString();
                }
            }

            // Already a primitive (string/int/bool/etc.)
            return value;
        }
    }
}
