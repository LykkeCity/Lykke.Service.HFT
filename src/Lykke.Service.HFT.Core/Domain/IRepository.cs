﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Domain
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task Add(TEntity entity);

        Task Add(IEnumerable<TEntity> items);

        Task Update(TEntity entity);

        Task Update(IEnumerable<TEntity> items);

        Task Delete(TEntity entity);

        Task Delete(IEnumerable<TEntity> entities);

        Task DeleteAsync(Expression<Func<TEntity, bool>> filter);

        IQueryable<TEntity> All();

        Task<TEntity> Get(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> Get(Guid id);

        IQueryable<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression);
    }
}
