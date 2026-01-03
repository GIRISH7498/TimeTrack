using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities;
using TimeTrack.Domain.Entities.NotificationModels;
using TimeTrack.Infrastructure.Identity;

namespace TimeTrack.Infrastructure.Persistence
{
    public class ApplicationDbContext
    : IdentityDbContext<AppUser, AppRole, int>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<UserOtp> UserOtps => Set<UserOtp>();
        public DbSet<NotificationCategory> NotificationCategories => Set<NotificationCategory>();
        public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
        public DbSet<NotificationAddressBook> NotificationAddressBooks => Set<NotificationAddressBook>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<NotificationUnsubscribe> NotificationUnsubscribes => Set<NotificationUnsubscribe>();
        public DbSet<PushDevice> PushDevices => Set<PushDevice>();
        public DbSet<NotificationEvent> NotificationEvents => Set<NotificationEvent>();
        public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();
        public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
        public DbSet<NotificationAttempt> NotificationAttempts => Set<NotificationAttempt>();
        public DbSet<NotificationOutbox> NotificationOutboxes => Set<NotificationOutbox>();
        public DbSet<NotificationAttachment> NotificationAttachments => Set<NotificationAttachment>();
        public DbSet<BellInboxItem> BellInboxItems => Set<BellInboxItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>(b =>
            {
                b.ToTable("Users");
                b.Property(u => u.FirstName).HasMaxLength(100);
                b.Property(u => u.LastName).HasMaxLength(100);
            });

            builder.Entity<AppRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            builder.Entity<Employee>(b =>
            {
                b.ToTable("Employees");

                b.HasKey(e => e.Id);

                b.Property(e => e.EmployeeCode)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.LastName)
                    .HasMaxLength(100);

                b.HasIndex(e => e.EmployeeCode)
                    .IsUnique();

                b.HasIndex(e => e.UserId)
                    .IsUnique(); // one Employee per User

                // 1:1: Employee -> AppUser
                b.HasOne<AppUser>()
                    .WithOne(u => u.Employee)
                    .HasForeignKey<Employee>(e => e.UserId)
                    .IsRequired();
            });

            builder.Entity<UserOtp>(b =>
            {
                b.ToTable("UserOtps");

                b.HasKey(o => o.Id);

                b.Property(o => o.Code)
                    .IsRequired()
                    .HasMaxLength(10);

                b.Property(o => o.Purpose)
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasOne<AppUser>()
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .IsRequired();
            });

            //Notificaton Models

            // Category
            builder.Entity<NotificationCategory> (b =>
            {
                b.ToTable("NotificationCategories");

                b.HasKey(c => c.CategoryId);

                b.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(c => c.AllowUnsubscribe)
                    .IsRequired();
            });

            // Template
            builder.Entity<NotificationTemplate>(b =>
            {
                b.ToTable("NotificationTemplates");

                b.HasKey(t => t.TemplateId);

                b.Property(t => t.TemplateKey)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(t => t.Body)
                    .IsRequired();

                b.Property(t => t.LanguageCode)
                    .HasMaxLength(10);

                b.Property(t => t.Subject)
                    .HasMaxLength(200);

                b.HasOne(t => t.Category)
                    .WithMany(c => c.Templates)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AddressBook
            builder.Entity<NotificationAddressBook>(b =>
            {
                b.ToTable("NotificationAddressBook");

                b.HasKey(a => a.AddressId);

                b.Property(a => a.Email)
                    .HasMaxLength(256);

                b.Property(a => a.PhoneE164)
                    .HasMaxLength(32);

                // optional useful indexes
                b.HasIndex(a => a.Email);
                b.HasIndex(a => a.PhoneE164);
            });

            // Preference
            builder.Entity<NotificationPreference>(b =>
            {
                b.ToTable("NotificationPreferences");

                b.HasKey(p => p.PreferenceId);

                b.HasOne(p => p.Category)
                    .WithMany(c => c.Preferences)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Unsubscribe
            builder.Entity<NotificationUnsubscribe>(b =>
            {
                b.ToTable("NotificationUnsubscribes");

                b.HasKey(u => u.UnsubscribeId);

                b.Property(u => u.Email)
                    .HasMaxLength(256);

                b.Property(u => u.PhoneE164)
                    .HasMaxLength(32);

                b.HasIndex(u => new { u.Email, u.CategoryId });
                b.HasIndex(u => new { u.PhoneE164, u.CategoryId });
                b.HasIndex(u => new { u.UserId, u.CategoryId });
            });

            // PushDevice
            builder.Entity<PushDevice>(b =>
            {
                b.ToTable("PushDevices");

                b.HasKey(d => d.DeviceId);

                b.Property(d => d.PushToken)
                    .IsRequired()
                    .HasMaxLength(512);

                b.HasIndex(d => new { d.UserId, d.Platform });
            });

            // NotificationEvent
            builder.Entity<NotificationEvent>(b =>
            {
                b.ToTable("NotificationEvents");

                b.HasKey(e => e.NotificationId);

                b.Property(e => e.TemplateKey)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(e => e.TemplateDataJson)
                    .IsRequired()
                    .HasDefaultValue("{}");

                b.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotificationRecipient
            builder.Entity<NotificationRecipient>(b =>
            {
                b.ToTable("NotificationRecipients");

                b.HasKey(r => r.RecipientId);

                b.Property(r => r.TargetKey)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(r => r.Email)
                    .HasMaxLength(256);

                b.Property(r => r.PhoneE164)
                    .HasMaxLength(32);

                b.HasOne(r => r.Notification)
                    .WithMany(e => e.Recipients)
                    .HasForeignKey(r => r.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // NotificationMessage
            builder.Entity<NotificationMessage>(b =>
            {
                b.ToTable("NotificationMessages");

                b.HasKey(m => m.MessageId); 

                b.HasOne(m => m.Notification)
                    .WithMany(e => e.Messages)
                    .HasForeignKey(m => m.NotificationId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(m => m.Recipient)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(m => m.RecipientId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(m => m.Template)
                    .WithMany()
                    .HasForeignKey(m => m.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.Property(m => m.Status)
                    .IsRequired();

                b.Property(m => m.ChannelId)
                    .IsRequired();
            });

            // NotificationAttempt
            builder.Entity<NotificationAttempt>(b =>
            {
                b.ToTable("NotificationAttempts");

                b.HasKey(a => a.AttemptId);

                b.Property(a => a.Provider)
                    .IsRequired()
                    .HasMaxLength(100);

                b.HasOne(a => a.Message)
                    .WithMany(m => m.Attempts)
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Outbox
            builder.Entity<NotificationOutbox>(b =>
            {
                b.ToTable("NotificationOutbox");

                b.HasKey(o => o.OutboxId);

                b.Property(o => o.EventType)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(o => o.AggregateId)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(o => o.IdempotencyKey)
                    .IsRequired()
                    .HasMaxLength(200);

                b.HasIndex(o => o.IdempotencyKey)
                    .IsUnique();
            });

            // BellInboxItem
            builder.Entity<BellInboxItem>(b =>
            {
                b.ToTable("BellInboxItems");

                b.HasKey(i => i.InboxId);

                b.Property(i => i.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(i => i.Body)
                    .IsRequired()
                    .HasMaxLength(1000);

                b.HasOne(i => i.Message)
                    .WithOne(m => m.BellInboxItem)
                    .HasForeignKey<BellInboxItem>(i => i.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(i => new { i.UserId, i.IsRead });
            });

            builder.Entity<NotificationAttachment>(b =>
            {
                b.ToTable("NotificationAttachments");

                b.HasKey(a => a.AttachmentId);

                b.Property(a => a.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property(a => a.ContentType)
                    .IsRequired()
                    .HasMaxLength(200);

                b.HasOne(a => a.Message)
                    .WithMany(m => m.Attachments)
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
