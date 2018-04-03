using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Lykke.Service.HFT.MongoRepositories
{
    public class MongoRepository<T> : IRepository<T> where T : class, IHasId
    {
        protected readonly IMongoDatabase Database;

        public MongoRepository(IMongoDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));

            MongoDefaults.GuidRepresentation = GuidRepresentation.Standard;
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
            return Database.GetCollection<T>(typeof(T).Name);
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

        public Task DeleteAsync(Expression<Func<T, bool>> filter)
        {
            return GetCollection().DeleteManyAsync(filter);
        }

        public async Task Delete(IEnumerable<T> entities)
        {
            await entities.ParallelForEachAsync(async item =>
            {
                await Delete(item).ConfigureAwait(false);
            }).ConfigureAwait(false);
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
    }
}
