using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;

public class DataRepository<TEntity, TKey> : DataRepositoryBase<TEntity, TKey> where TEntity : AggregateRoot<TKey> where TKey : notnull
{
    protected readonly AppDbContext _dataContext;
    private readonly ILogger<DataRepository<TEntity, TKey>> _logger;
    protected readonly DbSet<TEntity> _dbSet;

    public DataRepository(AppDbContext context, ILogger<DataRepository<TEntity, TKey>> logger)
    {
        _dataContext = context;
        _logger = logger;
        _dbSet = context.Set<TEntity>();
    }

    public override async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving {EntityType} with id {EntityId}.", typeof(TEntity).Name, id);

        var entity = await _dbSet.FindAsync([id], ct);

        if (entity is null)
        {
            _logger.LogDebug("{EntityType} with id {EntityId} was not found.", typeof(TEntity).Name, id);
        }

        return entity;
    }

    public override async Task<IReadOnlyList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null, CancellationToken ct = default)
    {
        _logger.LogDebug("GetAllAsync - Retrieving all entities of type {EntityType}.", typeof(TEntity).Name);

        IQueryable<TEntity> query = _dbSet;

        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        var entities = await query.ToListAsync(ct);

        _logger.LogDebug("GetAllAsync - Retrieved {EntityCount} entities of type {EntityType}.", entities.Count, typeof(TEntity).Name);

        return entities;
    }

    public override async Task<IReadOnlyList<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null, CancellationToken ct = default)
    {
        _logger.LogDebug("QueryAsync - Executing query for entities of type {EntityType}.", typeof(TEntity).Name);

        IQueryable<TEntity> query = _dbSet.Where(predicate);

        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        var entities = await query.ToListAsync(ct);

        _logger.LogDebug("QueryAsync - Query for {EntityType} returned {EntityCount} entities.", typeof(TEntity).Name, entities.Count);

        return entities;
    }

    public override async Task<TEntity?> QuerySingleAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null, CancellationToken ct = default)
    {
        _logger.LogDebug("Executing single-entity query for {EntityType}.", typeof(TEntity).Name);

        IQueryable<TEntity> query = _dbSet.Where(predicate);

        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        var entity = await query.SingleOrDefaultAsync(ct);

        if (entity is null)
        {
            _logger.LogDebug("Single-entity query for {EntityType} returned no entity.", typeof(TEntity).Name);

            return null;
        }

        _logger.LogDebug("Single-entity query returned {EntityType} with ID {EntityId}.", typeof(TEntity).Name, entity.Id);

        return entity;
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        _logger.LogDebug("Staging {EntityType} with ID {EntityId} for insertion.", typeof(TEntity).Name, entity.Id);

        await _dbSet.AddAsync(entity, ct);

        _logger.LogDebug("{EntityType} with ID {EntityId} was staged for insertion.", typeof(TEntity).Name, entity.Id);
    }

    public override Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _logger.LogDebug("Updating {EntityType} with ID {EntityId}.", typeof(TEntity).Name, entity.Id);

        _dbSet.Update(entity);

        _logger.LogDebug("{EntityType} with ID {EntityId} was staged for update.", typeof(TEntity).Name, entity.Id);

        return Task.CompletedTask;
    }

    public override async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
    {
        _logger.LogDebug("Deleting {EntityType} with ID {EntityId}.", typeof(TEntity).Name, id);

        var affectedRows = await _dbSet
            .Where(entity => entity.Id.Equals(id))
            .ExecuteDeleteAsync(ct);

        if (affectedRows == 0)
        {
            _logger.LogDebug("{EntityType} with ID {EntityId} was not found for deletion.", typeof(TEntity).Name, id);
            return false;
        }

        _logger.LogInformation("{EntityType} with ID {EntityId} was deleted. Affected rows: {AffectedRows}.", typeof(TEntity).Name, id, affectedRows);
        return true;
    }

    public override async Task SaveChangesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("DataRepository SaveChangesAsync - {EntityType}", typeof(TEntity).Name);

        var affectedEntries = await _dataContext.SaveChangesAsync(ct); 
       
    }
}
