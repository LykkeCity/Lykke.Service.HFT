using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task Add(TEntity entity);

        Task Add(IEnumerable<TEntity> items);

        Task Update(TEntity entity);

        Task Update(IEnumerable<TEntity> items);

        Task Delete(TEntity entity);

        Task DeleteAsync(IEnumerable<TEntity> entities);

        IQueryable<TEntity> All();

        Task<TEntity> Get(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> Get(Guid id);

        IQueryable<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<TEntity>> FilterAsync(Expression<Func<TEntity, bool>> expression, int? batchSize = null, int? limit = null);
    }
}
