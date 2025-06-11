// Purpose: Configures and exposes the database tables to the application
using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; } // Database table for students
        public DbSet<PaymentNotification> PaymentNotifications { get; set; } // Database table for payment notifications
        public DbSet<PaymentNotification> Payments { get; set; }


       

    }

    
}

