using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities.NotificationModels;
using TimeTrack.Domain.Enums;

namespace TimeTrack.Infrastructure.Email
{
    public class NotificationTemplateSeeder
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostEnvironment _env;
        private readonly ILogger<NotificationTemplateSeeder> _logger;

        public NotificationTemplateSeeder(
            IApplicationDbContext context,
            IHostEnvironment env,
            ILogger<NotificationTemplateSeeder> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            // 0) Ensure all known categories exist
            var existingCategories = await _context.NotificationCategories
                .AsNoTracking()
                .Where(c => NotificationCategoryNames.All.Contains(c.Name))
                .ToListAsync(cancellationToken);

            var categoriesByName = existingCategories
                .ToDictionary(c => c.Name, c => c);

            foreach (var categoryName in NotificationCategoryNames.All)
            {
                if (!categoriesByName.ContainsKey(categoryName))
                {
                    var allowUnsubscribe = categoryName != NotificationCategoryNames.Security; // example rule

                    var cat = new NotificationCategory
                    {
                        Name = categoryName,
                        AllowUnsubscribe = allowUnsubscribe
                    };

                    _context.NotificationCategories.Add(cat);
                    categoriesByName[categoryName] = cat;
                }
            }

            // Save if we added any categories
            await _context.SaveChangesAsync(cancellationToken);

            // 1) Resolve templates folder (your current infra path)
            var templatesFolder = Path.Combine(
                _env.ContentRootPath,
                "..",
                "TimeTrack.Infrastructure",
                "Email",
                "Templates");

            templatesFolder = Path.GetFullPath(templatesFolder);

            if (!Directory.Exists(templatesFolder))
            {
                _logger.LogWarning("Email templates folder does not exist: {Folder}", templatesFolder);
                return;
            }

            // 2) Load template files
            var files = Directory.GetFiles(templatesFolder, "*.html.liquid", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);    // e.g. "User.PasswordResetOtp.html.liquid"
                var templateKey = fileName.Replace(".html.liquid", "");

                var body = await File.ReadAllTextAsync(file, cancellationToken);

                // Determine category based on template key
                var categoryName = NotificationTemplateCategories.GetCategoryNameForTemplate(templateKey);

                if (!categoriesByName.TryGetValue(categoryName, out var category))
                {
                    _logger.LogWarning(
                        "Category {CategoryName} not found for template {TemplateKey}. Skipping.",
                        categoryName, templateKey);
                    continue;
                }

                var existing = await _context.NotificationTemplates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.ChannelId == NotificationChannelType.Email &&
                        t.TemplateKey == templateKey,
                        cancellationToken);

                if (existing is null)
                {
                    _logger.LogInformation("Seeding new email template {TemplateKey} in category {CategoryName}",
                        templateKey, categoryName);

                    var template = new NotificationTemplate
                    {
                        ChannelId = NotificationChannelType.Email,
                        CategoryId = category.CategoryId,
                        TemplateKey = templateKey,
                        Version = 1,
                        LanguageCode = "en",
                        Subject = GetSubjectFor(templateKey),
                        Body = body,
                        IsActive = true
                    };

                    _context.NotificationTemplates.Add(template);
                }
                else
                {
                    _logger.LogInformation("Updating email template {TemplateKey}", templateKey);
                    existing.Body = body;
                    existing.CategoryId = category.CategoryId; // keep category in sync too if you change mapping
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        private static string GetSubjectFor(string templateKey)
        {
            return templateKey switch
            {
                "User.PasswordReset" => "Password reset request for {{ firstName }}",
                _ => templateKey
            };
        }
    }
}
