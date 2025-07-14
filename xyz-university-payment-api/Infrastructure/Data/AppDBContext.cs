// Purpose: Configures and exposes the database tables to the application
using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;

namespace xyz_university_payment_api.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; } // Database table for students
        public DbSet<PaymentNotification> PaymentNotifications { get; set; } // Database table for payment notifications
        public DbSet<FeeSchedule> FeeSchedules { get; set; } // Database table for fee schedules
        public DbSet<StudentBalance> StudentBalances { get; set; } // Database table for student balances
        public DbSet<PaymentPlan> PaymentPlans { get; set; } // Database table for payment plans

        // Authorization entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuthorizationAuditLog> AuthorizationAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentNotification>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            // FeeSchedule configuration
            modelBuilder.Entity<FeeSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Semester).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AcademicYear).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Program).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TuitionFee).HasPrecision(18, 2);
                entity.Property(e => e.RegistrationFee).HasPrecision(18, 2);
                entity.Property(e => e.LibraryFee).HasPrecision(18, 2);
                entity.Property(e => e.LaboratoryFee).HasPrecision(18, 2);
                entity.Property(e => e.OtherFees).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => new { e.Semester, e.AcademicYear, e.Program }).IsUnique();
            });

            // StudentBalance configuration
            modelBuilder.Entity<StudentBalance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
                entity.Property(e => e.OutstandingBalance).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.StudentNumber, e.FeeScheduleId }).IsUnique();
                
                // Relationships
                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentNumber)
                    .HasPrincipalKey(s => s.StudentNumber)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.FeeSchedule)
                    .WithMany(f => f.StudentBalances)
                    .HasForeignKey(e => e.FeeScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PaymentPlan configuration
            modelBuilder.Entity<PaymentPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PlanType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.InstallmentAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
                entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                // Relationships
                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentNumber)
                    .HasPrincipalKey(s => s.StudentNumber)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.StudentBalance)
                    .WithMany()
                    .HasForeignKey(e => e.StudentBalanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Permission configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Resource).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
            });

            // UserRole configuration (many-to-many)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RolePermission configuration (many-to-many)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuthorizationAuditLog configuration
            modelBuilder.Entity<AuthorizationAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
            });
        }
    }

    // Design-time factory for EF tools
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use SQL Server with connection string from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

