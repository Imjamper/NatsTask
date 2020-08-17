using System;
using LiteDB;
using NatsTask.Common.Entity;
using NatsTask.Common.Repository;

namespace NatsTask.Common.Database
{
    public interface ILiteUnitOfWork : IDisposable
    {
        void Transaction(Action<ILiteUnitOfWork> body);

        bool TransactionSaveChanges(Action<ILiteUnitOfWork> body);

        IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;

        ILiteCollection<TEntity> Collection<TEntity>() where TEntity : class;

        void DeleteAll<TEntity>();
    }
}
