// Purpose: Unit of Work implementation for managing repositories and transactions
using Microsoft.EntityFrameworkCore.Storage;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private IGenericRepository<Student>? _students;
        private IGenericRepository<PaymentNotification>? _payments;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Repository properties
        public IGenericRepository<Student> Students
        {
            get
            {
                _students ??= new GenericRepository<Student>(_context);
                return _students;
            }
        }

        public IGenericRepository<PaymentNotification> Payments
        {
            get
            {
                _payments ??= new GenericRepository<PaymentNotification>(_context);
                return _payments;
            }
        }

        // Generic repository access
        public IGenericRepository<T> Repository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        // Transaction management
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Dispose pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }
} 