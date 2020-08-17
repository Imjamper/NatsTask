﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using NatsTask.Common.Entity;

namespace NatsTask.Common.Repository
{
    public interface IRepository<TEntity> where TEntity : IEntity
    {
        void Add(IList<TEntity> items);
        long Add(TEntity item);
        void DeleteAll();
        void Update(TEntity item);
        void Delete(TEntity item);
        IList<TEntity> Find(Expression<Func<TEntity, bool>> expression);
        TEntity FindOne(Expression<Func<TEntity, bool>> expression);
        IList<TEntity> FindAll();
        TEntity LoadOrNull(int id);
        ILiteCollection<TEntity> Collection { get; }
    }
}
