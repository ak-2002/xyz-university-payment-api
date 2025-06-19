// Purpose: Generic repository interface that defines the contract for all data access operations
// This follows the Repository Pattern and provides a consistent interface for CRUD operations
namespace xyz_university_payment_api.Interfaces
{
    /// <summary>
    /// Generic repository interface that provides CRUD operations for any entity type
    /// This interface follows the Repository Pattern and implements the Dependency Inversion Principle
    /// </summary>
    /// <typeparam name="T">The entity type that this repository will handle</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities of type T from the database
        /// </summary>
        /// <returns>An IEnumerable containing all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Retrieves an entity by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the entity</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity with generated ID</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the database
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Removes an entity from the database
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <returns>True if the entity was successfully removed</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Checks if an entity with the specified ID exists
        /// </summary>
        /// <param name="id">The unique identifier to check</param>
        /// <returns>True if the entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        Task<int> SaveChangesAsync();
    }
} 