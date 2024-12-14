using System.Data;

namespace Core.Abstractions;

public interface IDbSession : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; set; }
    void Close();
}

public interface IUnitOfWork : IDisposable
{
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    void Commit();
    void Rollback();
}
