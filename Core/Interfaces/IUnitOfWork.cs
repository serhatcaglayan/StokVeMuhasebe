using System;

namespace coreAden.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        int SaveChanges();
        void BeginTransaction();

         void Commit();
        void Rollback();
       


    }
}
