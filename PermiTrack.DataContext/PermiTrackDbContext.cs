using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext.Entites;

namespace PermiTrack.DataContext;

public class PermiTrackDbContext : DbContext
{
    public PermiTrackDbContext(DbContextOptions<PermiTrackDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<HttpAuditLog> HttpAuditLogs => Set<HttpAuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Sessions> Sessions => Set<Sessions>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        });

        // Role entity configuration with hierarchy support
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(500);

            // Self-referencing relationship: Role -> ParentRole
            entity.HasOne(r => r.ParentRole)
                .WithMany(r => r.SubRoles)
                .HasForeignKey(r => r.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict); // Do not delete children automatically
        });
            
        // Permission entity configuration with unique constraint
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Resource).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Action).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);

            // Unique index: (Resource, Action) combination must be unique
            entity.HasIndex(p => new { p.Resource, p.Action })
                .IsUnique()
                .HasDatabaseName("IX_Permission_Resource_Action_Unique");
        });

        // UserRole configuration (Many-to-Many: User to Role)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);
            
            // Composite unique index: (UserId, RoleId)
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique()
                .HasDatabaseName("IX_UserRole_UserId_RoleId_Unique");

            // Primary relationships
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit field - no cascade delete to prevent conflicts
            entity.HasOne(ur => ur.AssignedByUser)
                .WithMany()
                .HasForeignKey(ur => ur.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict) // Important: No cascade delete
                .IsRequired(false);
        });

        // RolePermission configuration (Many-to-Many: Role to Permission)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => rp.Id);
            
            entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermission_RoleId_PermissionId_Unique");

            // Primary relationships
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit field - no cascade delete to prevent conflicts
            entity.HasOne(rp => rp.GrantedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict) // Important: No cascade delete
                .IsRequired(false);
        });

        // AccessRequest entity configuration
        modelBuilder.Entity<AccessRequest>(entity =>
        {
            entity.HasKey(ar => ar.Id);

            // Primary relationship: User who requested
            entity.HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete the request

            // Requested role
            entity.HasOne(ar => ar.RequestedRole)
                .WithMany()
                .HasForeignKey(ar => ar.RequestedRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Audit fields - no cascade delete to prevent conflicts
            // ApprovedBy - Restrict to avoid multiple cascade paths
            entity.HasOne(ar => ar.ApprovedByUser)
                .WithMany()
                .HasForeignKey(ar => ar.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict) // Important: No cascade delete
                .IsRequired(false);

            // RejectedBy - Restrict to avoid multiple cascade paths
            entity.HasOne(ar => ar.RejectedByUser)
                .WithMany()
                .HasForeignKey(ar => ar.RejectedBy)
                .OnDelete(DeleteBehavior.Restrict) // Important: No cascade delete
                .IsRequired(false);

            // Workflow relationship
            entity.HasOne(ar => ar.Workflow)
                .WithMany()
                .HasForeignKey(ar => ar.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ar => ar.CurrentStep)
                .WithMany()
                .HasForeignKey(ar => ar.CurrentStepId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.Property(ar => ar.Status).HasMaxLength(50).IsRequired();
            entity.Property(ar => ar.RequestedPermissions).HasMaxLength(2000);
        });

        // ApprovalWorkflow entity configuration
        modelBuilder.Entity<ApprovalWorkflow>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).HasMaxLength(200).IsRequired();
            entity.Property(w => w.Description).HasMaxLength(1000);
        });

        // ApprovalStep entity configuration
        modelBuilder.Entity<ApprovalStep>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.Workflow)
                .WithMany(w => w.Steps)
                .HasForeignKey(s => s.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.ApproverRole)
                .WithMany(r => r.ApprovalSteps)
                .HasForeignKey(s => s.ApproverRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.StepName).HasMaxLength(200).IsRequired();
        });

        // AuditLog entity configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.ResourceType).HasMaxLength(100);
            entity.Property(a => a.IpAddress).HasMaxLength(50);
            entity.Property(a => a.UserAgent).HasMaxLength(500);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // HttpAuditLog entity configuration
        modelBuilder.Entity<HttpAuditLog>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Method).HasMaxLength(10).IsRequired();
            entity.Property(h => h.Path).HasMaxLength(500).IsRequired();
            entity.Property(h => h.QueryString).HasMaxLength(2000);
            entity.Property(h => h.IpAddress).HasMaxLength(50).IsRequired();
            entity.Property(h => h.UserAgent).HasMaxLength(500);
            entity.Property(h => h.Username).HasMaxLength(100);
            entity.Property(h => h.AdditionalInfo).HasMaxLength(2000);

            entity.HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better query performance
            entity.HasIndex(h => h.Timestamp);
            entity.HasIndex(h => h.UserId);
            entity.HasIndex(h => h.StatusCode);
            entity.HasIndex(h => new { h.Method, h.Path });
        });
    }
}
