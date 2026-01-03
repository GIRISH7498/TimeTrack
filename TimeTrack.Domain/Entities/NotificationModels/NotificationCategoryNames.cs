namespace TimeTrack.Domain.Entities.NotificationModels
{
    public static class NotificationCategoryNames
    {
        public const string General = "General";
        public const string Security = "Security";
        public const string TimeTracking = "TimeTracking";

        // convenient list of all category names we care about
        public static readonly IReadOnlyList<string> All =
            new[] { General, Security, TimeTracking };
    }

    public static class NotificationTemplateCategories
    {
        /// <summary>
        /// Maps a template key (e.g. "User.PasswordReset") to a category name.
        /// If unknown, falls back to General.
        /// </summary>
        public static string GetCategoryNameForTemplate(string templateKey) =>
            templateKey switch
            {
                // Security-related templates
                "User.PasswordReset" => NotificationCategoryNames.Security,

                // Time tracking templates (examples)
                "TimeEntry.Created" => NotificationCategoryNames.TimeTracking,
                "TimeEntry.Approved" => NotificationCategoryNames.TimeTracking,

                // Default
                _ => NotificationCategoryNames.General
            };
    }
}
