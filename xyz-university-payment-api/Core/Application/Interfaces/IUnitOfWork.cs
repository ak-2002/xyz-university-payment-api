// Purpose: Unit of Work interface for managing transactions across repositories
using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IGenericRepository<Student> Students { get; }
        IGenericRepository<PaymentNotification> Payments { get; }

        // Transaction management
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();

        // Generic repository access
        IGenericRepository<T> Repository<T>() where T : class;
    }
} 