using Abp.Json;
using Abp.OpenIddict.Applications;
using Abp.OpenIddict.Authorizations;
using Abp.OpenIddict.EntityFrameworkCore;
using Abp.OpenIddict.Scopes;
using Abp.OpenIddict.Tokens;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CadentManagement.Authorization.Delegation;
using CadentManagement.Authorization.Roles;
using CadentManagement.Authorization.Session;
using CadentManagement.Authorization.Users;
using CadentManagement.Chat;
using CadentManagement.Editions;
using CadentManagement.ExtraProperties;
using CadentManagement.Friendships;
using CadentManagement.MultiTenancy;
using CadentManagement.MultiTenancy.Accounting;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.RateLimiting;
using CadentManagement.Storage;
using CadentManagement.TimeTracking.Projects;
using CadentManagement.TimeTracking.Tasks;
using CadentManagement.TimeTracking.TimeEntries;

namespace CadentManagement.EntityFrameworkCore;

public class CadentManagementDbContext : AbpZeroDbContext<Tenant, Role, User, CadentManagementDbContext>, IOpenIddictDbContext
{
    /* Define an IDbSet for each entity of the application */

    public virtual DbSet<OpenIddictApplication> Applications { get; }

    public virtual DbSet<OpenIddictAuthorization> Authorizations { get; }

    public virtual DbSet<OpenIddictScope> Scopes { get; }

    public virtual DbSet<OpenIddictToken> Tokens { get; }

    public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

    public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

    public virtual DbSet<SubscriptionPaymentProduct> SubscriptionPaymentProducts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<UserDelegation> UserDelegations { get; set; }

    public virtual DbSet<RecentPassword> RecentPasswords { get; set; }

    public virtual DbSet<UserAccountLink> UserAccountLinks { get; set; }

    public virtual DbSet<UserExternalLoginInfo> UserExternalLoginInfos { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<RateLimitPolicy> RateLimitPolicies { get; set; }

    // Time Tracking
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectBudgetTracking> ProjectBudgetTrackings { get; set; }
    public virtual DbSet<ProjectTask> ProjectTasks { get; set; }
    public virtual DbSet<TaskBudgetTracking> TaskBudgetTrackings { get; set; }
    public virtual DbSet<TimeEntry> TimeEntries { get; set; }

    public CadentManagementDbContext(DbContextOptions<CadentManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BinaryObject>(b => { b.HasIndex(e => new { e.TenantId }); });

        modelBuilder.Entity<SubscribableEdition>(b =>
        {
            b.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
            b.Property(e => e.AnnualPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SubscriptionPayment>(x =>
        {
            x.Property(u => u.ExtraProperties)
                .HasConversion(
                    d => d.ToJsonString(false, false),
                    s => s.FromJsonString<ExtraPropertyDictionary>()
                )
                .Metadata.SetValueComparer(new ValueComparer<ExtraPropertyDictionary>(
                    (c1, c2) => c1.ToJsonString(false, false) == c2.ToJsonString(false, false),
                    c => c.ToJsonString(false, false).GetHashCode(),
                    c => c.ToJsonString(false, false).FromJsonString<ExtraPropertyDictionary>()
                ));
        });

        modelBuilder.Entity<SubscriptionPaymentProduct>(x =>
        {
            x.Property(u => u.ExtraProperties)
                .HasConversion(
                    d => d.ToJsonString(false, false),
                    s => s.FromJsonString<ExtraPropertyDictionary>()
                )
                .Metadata.SetValueComparer(new ValueComparer<ExtraPropertyDictionary>(
                    (c1, c2) => c1.ToJsonString(false, false) == c2.ToJsonString(false, false),
                    c => c.ToJsonString(false, false).GetHashCode(),
                    c => c.ToJsonString(false, false).FromJsonString<ExtraPropertyDictionary>()
                ));

            x.Property(e => e.Amount).HasPrecision(18, 2);
            x.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserExternalLoginInfo>(b =>
        {
            b.ToTable("AppUserExternalLoginInfos");
            b.HasIndex(e => new { e.TenantId, e.UserId, e.LoginProvider }).IsUnique().HasFilter(null);
            b.Property(e => e.LoginProvider).HasMaxLength(UserExternalLoginInfo.MaxLoginProviderLength).IsRequired();
            b.Property(e => e.EmailAddress).HasMaxLength(UserExternalLoginInfo.MaxEmailAddressLength).IsRequired();
            b.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
            b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
            b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
            b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
        });

        modelBuilder.Entity<Friendship>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.UserId });
            b.HasIndex(e => new { e.TenantId, e.FriendUserId });
            b.HasIndex(e => new { e.FriendTenantId, e.UserId });
            b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
        });

        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasIndex(e => new { e.SubscriptionEndDateUtc });
            b.HasIndex(e => new { e.CreationTime });
        });

        modelBuilder.Entity<SubscriptionPayment>(b =>
        {
            b.HasIndex(e => new { e.Status, e.CreationTime });
            b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
        });

        modelBuilder.Entity<UserDelegation>(b =>
        {
            b.HasIndex(e => new { e.TenantId, e.SourceUserId });
            b.HasIndex(e => new { e.TenantId, e.TargetUserId });
        });

		modelBuilder.Entity<UserAccountLink>(b =>
		{
			b.HasIndex(e => new { e.UserAccountId, e.LinkedUserAccountId }).IsUnique();
		});

        modelBuilder.Entity<UserSession>(b =>
        {
            b.HasIndex(e => new { e.UserId, e.IsActive });
            b.HasIndex(e => e.SessionToken).IsUnique();
            b.HasIndex(e => new { e.TenantId, e.UserId });
        });

        modelBuilder.Entity<RateLimitPolicy>(b =>
        {
            b.HasIndex(e => e.Name);
            b.HasIndex(e => e.IsEnabled);
        });

        // Time Tracking
        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("TT_Projects");
            b.HasIndex(e => new { e.TenantId, e.Status });
            b.Property(e => e.BudgetHours).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProjectBudgetTracking>(b =>
        {
            b.ToTable("TT_ProjectBudgetTrackings");
            b.HasOne(e => e.Project)
                .WithOne(e => e.BudgetTracking)
                .HasForeignKey<ProjectBudgetTracking>(e => e.ProjectId);
            b.Property(e => e.TotalBudgetHours).HasPrecision(18, 2);
            b.Property(e => e.UsedHours).HasPrecision(18, 2);
            b.Property(e => e.RemainingHours).HasPrecision(18, 2);
            b.Ignore(e => e.UtilizationPercentage);
        });

        modelBuilder.Entity<ProjectTask>(b =>
        {
            b.ToTable("TT_Tasks");
            b.HasIndex(e => new { e.TenantId, e.ProjectId });
            b.HasIndex(e => e.ParentTaskId);
            b.HasOne(e => e.Project)
                .WithMany(e => e.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.ParentTask)
                .WithMany(e => e.SubTasks)
                .HasForeignKey(e => e.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(e => e.BudgetHours).HasPrecision(18, 2);
        });

        modelBuilder.Entity<TaskBudgetTracking>(b =>
        {
            b.ToTable("TT_TaskBudgetTrackings");
            b.HasOne(e => e.Task)
                .WithOne(e => e.BudgetTracking)
                .HasForeignKey<TaskBudgetTracking>(e => e.TaskId);
            b.Property(e => e.TotalBudgetHours).HasPrecision(18, 2);
            b.Property(e => e.UsedHours).HasPrecision(18, 2);
            b.Property(e => e.RemainingHours).HasPrecision(18, 2);
            b.Ignore(e => e.UtilizationPercentage);
        });

        modelBuilder.Entity<TimeEntry>(b =>
        {
            b.ToTable("TT_TimeEntries");
            b.HasIndex(e => new { e.TenantId, e.ProjectId, e.StartTime });
            b.HasIndex(e => new { e.TenantId, e.UserId, e.StartTime });
            b.HasIndex(e => e.TaskId);
            b.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Ignore(e => e.DurationHours);
        });

        modelBuilder.ConfigureOpenIddict();
    }
}
