using Core.Abstractions;
using Core.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure;

/// <summary>
/// Represents a database session.
/// </summary>
public sealed class DbSession : IDbSession, IDisposable
{
    /// <summary>
    /// Gets the database connection.
    /// </summary>
    public IDbConnection Connection { get; }

    /// <summary>
    /// Gets or sets the database transaction.
    /// </summary>
    public IDbTransaction Transaction { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbSession"/> class.
    /// </summary>
    public DbSession()
    {
        var connectionString = AppSettings.ConnectionStrings?.DBConnection;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        Connection = new SqlConnection(connectionString);
        Connection.Open();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbSession"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public DbSession(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        Connection = new SqlConnection(connectionString);
        Connection.Open();
    }

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    public void Close()
    {
        if (Connection.State != ConnectionState.Closed)
        {
            Connection.Close();
        }
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="DbSession"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
                Connection.Dispose();
            }
        }
    }

    ~DbSession()
    {
        Dispose(false);
    }
}

/// <summary>
/// Represents a unit of work.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbSession _session;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="session">The database session.</param>
    public UnitOfWork(IDbSession session)
    {
        _session = session;
    }

    /// <summary>
    /// Begins a transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_session.Transaction == null)
        {
            _session.Transaction = _session.Connection.BeginTransaction(isolationLevel);
        }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public void Commit()
    {
        if (_session.Transaction != null)
        {
            _session.Transaction.Commit();
        }

        Dispose();
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public void Rollback()
    {
        if (_session.Transaction != null)
        {
            _session.Transaction.Rollback();
        }

        Dispose();
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="UnitOfWork"/> class.
    /// </summary>
    public void Dispose() => _session.Transaction?.Dispose();
}

