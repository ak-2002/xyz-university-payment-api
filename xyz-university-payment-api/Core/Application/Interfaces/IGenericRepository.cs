// Purpose: Generic repository interface for common CRUD operations
using System.Linq.Expressions;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Query operations
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // Pagination
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);

        // Include related entities
        IQueryable<T> Query();
        IQueryable<T> Query(Expression<Func<T, bool>> predicate);

        // Bulk operations
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task DeleteRangeAsync(Expression<Func<T, bool>> predicate);

        // Save changes
        Task<int> SaveChangesAsync();
    }
} 