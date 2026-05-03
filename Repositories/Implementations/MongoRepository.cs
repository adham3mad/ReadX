using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Settings;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ReadX.Api.Repositories.Implementations;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

        var collectionName = typeof(T).Name;
        if (collectionName.EndsWith("s") == false)
        {
            collectionName += "s"; // basic pluralization
        }

        _collection = mongoDatabase.GetCollection<T>(collectionName);
    }

    public async Task<List<T>> GetAllAsync(FilterDefinition<T> filter, int page = 1, int limit = 20)
    {
        return await _collection.Find(filter)
                                .Skip((page - 1) * limit)
                                .Limit(limit)
                                .ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, int page = 1, int limit = 20)
    {
        return await _collection.Find(filter)
                                .Skip((page - 1) * limit)
                                .Limit(limit)
                                .ToListAsync();
    }

    public async Task<long> CountAsync(FilterDefinition<T> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T?> GetOneAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T?> GetOneAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task RemoveAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter);
    }
}
