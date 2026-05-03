using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ReadX.Api.Repositories.Interfaces;

public interface IMongoRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(FilterDefinition<T> filter, int page = 1, int limit = 20);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, int page = 1, int limit = 20);
    Task<long> CountAsync(FilterDefinition<T> filter);
    Task<long> CountAsync(Expression<Func<T, bool>> filter);
    Task<T?> GetByIdAsync(string id);
    Task<T?> GetOneAsync(FilterDefinition<T> filter);
    Task<T?> GetOneAsync(Expression<Func<T, bool>> filter);
    Task CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task RemoveAsync(string id);
}
