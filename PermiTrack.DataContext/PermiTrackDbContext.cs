using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext.Entites;
using System.Security.AccessControl;

namespace PermiTrack.DataContext;

public class PermiTrackDbContext : DbContext
{
    public PermiTrackDbContext(DbContextOptions<PermiTrackDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Sessions> Sessions => Set<Sessions>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);

        // Users
        // UserRoles
        m.Entity<UserRole>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.AssignedAt).HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.User)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Role)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        });

        // RolePermissions
        m.Entity<RolePermission>(e =>
        {
            e.ToTable("RolePermissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.GrantedAt).HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.Role)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Permission)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.GrantedByUser)
                .WithMany()
                .HasForeignKey(x => x.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        });

        // ApprovalSteps
        m.Entity<ApprovalStep>(e =>
        {
            e.ToTable("ApprovalSteps");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(x => x.Workflow)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ApproverRole)
                .WithMany()
                .HasForeignKey(x => x.ApproverRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.WorkflowId, x.StepOrder }).IsUnique();
        });

        // AccessRequests
        m.Entity<AccessRequest>(e =>
        {
            e.ToTable("AccessRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.Property(x => x.RequestedPermissions).HasColumnType("NVARCHAR(MAX)");

            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RequestedRole).WithMany().HasForeignKey(x => x.RequestedRoleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedBy).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedBy).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Workflow).WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CurrentStep).WithMany().HasForeignKey(x => x.CurrentStepId).OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLogs
        m.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Action).IsRequired().HasMaxLength(100);
            e.Property(x => x.ResourceType).IsRequired().HasMaxLength(100);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(x => x.OldValues).HasColumnType("NVARCHAR(MAX)");
            e.Property(x => x.NewValues).HasColumnType("NVARCHAR(MAX)");
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.Property(x => x.UserAgent).HasMaxLength(500);

            e.HasOne(x => x.User)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Notifications
        m.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(x => x.RelatedResourceType).HasMaxLength(100);

            e.HasOne(x => x.User)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Sessions
        m.Entity<Sessions>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.TokenHash).IsRequired().HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.Property(x => x.UserAgent).HasMaxLength(500);

            e.HasOne(x => x.User)
                .WithMany() // <-- EZ!
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.TokenHash).IsUnique();
        });

    }
}
