using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;

namespace NatsTask.Common.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly ILiteUnitOfWork UnitOfWork;

        public Repository(ILiteUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public void Add(IList<TEntity> items)
        {
            foreach (var entity in items) Collection.Insert(entity);
        }

        public long Add(TEntity item)
        {
            var bsonValue = Collection.Insert(item);

            return bsonValue.AsInt64;
        }

        public void Delete(TEntity item)
        {
            Collection.Delete(new BsonValue(item.Id));
        }

        public void DeleteAll()
        {
            UnitOfWork.DeleteAll<TEntity>();
        }

        public IList<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            return Collection.Find(expression).ToList();
        }

        public IList<TEntity> FindAll()
        {
            return Collection.FindAll().ToList();
        }

        public TEntity LoadOrNull(int id)
        {
            return Collection.FindById(id);
        }

        public void Update(TEntity item)
        {
            Collection.Update(item);
        }

        public ILiteCollection<TEntity> Collection => UnitOfWork.Collection<TEntity>();

        public TEntity FindOne(Expression<Func<TEntity, bool>> expression)
        {
            return Collection.FindOne(expression);
        }
    }
}