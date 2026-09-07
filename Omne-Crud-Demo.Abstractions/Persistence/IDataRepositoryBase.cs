using System.Linq.Expressions;

namespace Omne_Crud_Demo.Abstractions;

/// <summary>
/// Generic repository interface for aggregate roots entities.
/// Implementations handle persistence and domain event dispatching.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type</typeparam>
/// <typeparam name="TKey">The type of the aggregate identifier</typeparam>
public interface IDataRepositoryBase<TEntity, TKey>
    where TEntity : AggregateRoot<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Gets all entities with optional ordering and includes.
    /// </summary>
    /// <param name="orderBy">Optional ordering function</param>
    /// <param name="includes">Optional navigation properties to include</param>
    /// <param name="ct">Cancellation token</param>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Queries entities matching a predicate with optional ordering and includes.
    /// </summary>
    /// <param name="predicate">Filter expression</param>
    /// <param name="orderBy">Optional ordering function</param>
    /// <param name="includes">Optional navigation properties to include</param>
    /// <param name="ct">Cancellation token</param>
    Task<IReadOnlyList<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Queries a single entity matching a predicate with optional ordering and includes.
    /// Returns the first match or null if none found.
    /// </summary>
    /// <param name="predicate">Filter expression</param>
    /// <param name="orderBy">Optional ordering function</param>
    /// <param name="includes">Optional navigation properties to include</param>
    /// <param name="ct">Cancellation token</param>
    Task<TEntity?> QuerySingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    Task DeleteAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Persists all changes and dispatches domain events.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
