using coreAden.Core.Interfaces;
using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace coreAden.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly adenEntities _context;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed = false;

        private DbContextTransaction _transaction; // 🔹 aktif transaction'ı tutar

        public UnitOfWork(adenEntities context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
                return (IRepository<T>)_repositories[typeof(T)];

            var repository = new Repository<T>(_context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        // 🔹 Transaction başlatır
        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        // 🔹 Transaction onaylar (Commit)
        public void Commit()
        {
            try
            {
                _context.SaveChanges();
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw; // hatayı dışarı fırlat (loglanabilsin)
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        // 🔹 Transaction geri alır (Rollback)
        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
