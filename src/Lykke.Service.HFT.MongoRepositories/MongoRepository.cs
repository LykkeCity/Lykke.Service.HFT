using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Lykke.Service.HFT.MongoRepositories
{
    public class MongoRepository<T> : IRepository<T> where T : class, IHasId
    {
        protected readonly IMongoDatabase Database;
        private readonly ILog _log;
        private int _rpsLimit;
        private readonly string _collectionName = typeof(T).Name;

        public MongoRepository(IMongoDatabase database, ILog log, int rpsLimit = 100)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            _log = log.CreateComponentScope(nameof(MongoRepository<T>));
            _rpsLimit = rpsLimit;

            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.SetIdMember(cm.GetMemberMap(x => x.Id)
                        .SetIdGenerator(GuidGenerator.Instance));
                });
            }
        }

        protected IMongoCollection<T> GetCollection()
        {
            return Database.GetCollection<T>(_collectionName);
        }

        public async Task<T> Get(Guid id)
        {
            return await Get(x => x.Id == id).ConfigureAwait(false);
        }

        public async Task Add(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            await GetCollection().InsertOneAsync(entity).ConfigureAwait(false);
        }

        public async Task Add(IEnumerable<T> items)
        {
            var documents = items.ToList();
            foreach (var document in documents)
            {
                document.Id = Guid.NewGuid();
            }
            await GetCollection().InsertManyAsync(documents).ConfigureAwait(false);
        }

        public async Task Update(T entity)
        {
            await GetCollection().ReplaceOneAsync(new BsonDocument("_id", entity.Id), entity).ConfigureAwait(false);
        }

        public async Task Update(IEnumerable<T> items)
        {
            await items.ParallelForEachAsync(async item =>
            {
                await Update(item).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task Delete(T entity)
        {
            await GetCollection().DeleteOneAsync(new BsonDocument("_id", entity.Id)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(IEnumerable<T> entities)
        {
            // this implementation takes care of CosmosDB rate limiting, starting from 100 delete operations per second
            var ids = entities.Select(x => x.Id).ToList();
            var chunks = ids.ChunkBy(_rpsLimit);
            var sw = new Stopwatch();
            foreach (var chunk in chunks)
            {
                sw.Restart();
                try
                {
                    await GetCollection().DeleteManyAsync(x => chunk.Contains(x.Id));
                }
                catch (MongoCommandException exception)
                {
                    if (exception.Message.Contains("Request rate is large"))
                    {
                        _rpsLimit = (int)(_rpsLimit * 0.8);
                        _log.WriteWarning(nameof(DeleteAsync), "Request rate is large", $"Decreasing rps: {_rpsLimit}", exception);
                    }
                    throw;
                }
                if (sw.ElapsedMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)sw.ElapsedMilliseconds);
                }
            }
            sw.Stop();
        }

        public IQueryable<T> All()
        {
            return GetCollection().AsQueryable();
        }

        public async Task<T> Get(Expression<Func<T, bool>> expression)
        {
            return await GetCollection().Find(expression).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public IQueryable<T> FilterBy(Expression<Func<T, bool>> expression)
        {
            return GetCollection().Find(expression).ToList().AsQueryable();
        }

        public async Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> expression, int? take = null)
        {
            var result = await GetCollection().FindAsync(expression, new FindOptions<T> { Limit = take, BatchSize = take });
            return result.ToEnumerable();
        }
    }
}
