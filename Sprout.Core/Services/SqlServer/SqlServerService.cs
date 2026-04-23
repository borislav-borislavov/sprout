using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Sprout.Core.Services.SqlServer
{
    public class SqlServerService : IAsyncDisposable
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;
        private SqlTransaction? _transaction;

        public SqlServerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task OpenConnectionAsync()
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
        }

        public async Task CloseConnectionAsync()
        {
            if (_connection is null)
                return;

            if (_transaction is not null)
                throw new InvalidOperationException(
                    "Cannot close the connection while a transaction is still active. " +
                    $"Call {nameof(CommitTransactionAsync)} or {nameof(RollbackTransactionAsync)} first.");

            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        public async Task BeginTransactionAsync()
        {
            EnsureConnectionOpen();

            if (_transaction is not null)
                throw new InvalidOperationException("A transaction is already active. Commit or roll it back before starting a new one.");

            _transaction = (SqlTransaction)await _connection!.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            EnsureTransactionActive();
            await _transaction!.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            EnsureTransactionActive();

            try
            {
                await _transaction!.RollbackAsync();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task ExecuteAsync(string sql, object? param = null)
        {
            EnsureConnectionOpen();
            await _connection!.ExecuteAsync(sql, param, transaction: _transaction);
        }

        public async Task<List<T>> QueryAsync<T>(string sql, object? param = null)
        {
            EnsureConnectionOpen();
            var result = await _connection!.QueryAsync<T>(sql, param, transaction: _transaction);
            return result.AsList();
        }

        public async ValueTask DisposeAsync() => await CloseConnectionAsync();

        private void EnsureConnectionOpen()
        {
            if (_connection is null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException(
                    $"Connection is not open. Call {nameof(OpenConnectionAsync)} before executing any commands.");
        }

        private void EnsureTransactionActive()
        {
            if (_transaction is null)
                throw new InvalidOperationException(
                    $"No active transaction. Call {nameof(BeginTransactionAsync)} first.");
        }
    }
}
