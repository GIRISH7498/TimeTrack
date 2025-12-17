using Microsoft.EntityFrameworkCore;
using TimeTrack.Domain.Entities;
using TimeTrack.Domain.Entities.NotificationModels;

namespace TimeTrack.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Employee> Employees { get; }
        DbSet<UserOtp> UserOtps { get; }
        DbSet<NotificationCategory> NotificationCategories { get; }
        DbSet<NotificationTemplate> NotificationTemplates { get; }
        DbSet<NotificationAddressBook> NotificationAddressBooks { get; }
        DbSet<NotificationPreference> NotificationPreferences { get; }
        DbSet<NotificationUnsubscribe> NotificationUnsubscribes { get; }
        DbSet<PushDevice> PushDevices { get; }
        DbSet<NotificationEvent> NotificationEvents { get; }
        DbSet<NotificationRecipient> NotificationRecipients { get; }
        DbSet<NotificationMessage> NotificationMessages { get; }
        DbSet<NotificationAttempt> NotificationAttempts { get; }
        DbSet<NotificationOutbox> NotificationOutboxes { get; }
        DbSet<BellInboxItem> BellInboxItems { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
