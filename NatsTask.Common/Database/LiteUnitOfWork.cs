using System;
using System.Collections.Generic;
using LiteDB;
using NatsTask.Common.Entity;
using NatsTask.Common.Repository;

namespace NatsTask.Common.Database
{
    public class LiteUnitOfWork : ILiteUnitOfWork
    {
        private readonly LiteDatabase _database;
        private Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

        public LiteUnitOfWork()
        {
            _database = LiteDb.ReadWrite;
        }

        public void Dispose()
        {
            _repositories.Clear();
            _repositories = null;
            GC.SuppressFinalize(this);
        }

        public void Transaction(Action<ILiteUnitOfWork> body)
        {
            _database.BeginTrans();

            try
            {
                body.Invoke(this);
                _database.Commit();
            }
            catch
            {
                _database.Rollback();
            }
        }

        public bool TransactionSaveChanges(Action<ILiteUnitOfWork> body)
        {
            _database.BeginTrans();
            try
            {
                body.Invoke(this);
                _database.Commit();
                return true;
            }
            catch
            {
                _database.Rollback();
                return false;
            }
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
                return _repositories[typeof(TEntity)] as Repository<TEntity>;

            var repository = Activator.CreateInstance(typeof(Repository<TEntity>), this) as IRepository<TEntity>;
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public ILiteCollection<TEntity> Collection<TEntity>() where TEntity : class
        {
            return _database.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        public void DeleteAll<TEntity>()
        {
            _database.DropCollection(typeof(TEntity).Name);
        }
    }

    public class UnitOfWork : LiteUnitOfWork
    {
    }
}