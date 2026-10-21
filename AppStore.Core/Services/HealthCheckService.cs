using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace AppStore.Core.Services
{
    /// <summary>
    /// Проверка состояния зависимостей приложения (health check).
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Проверяет доступность базы данных: открывает соединение и выполняет
        /// облегчённый запрос. Таймаут ограничен, чтобы висящая БД не блокировала UI.
        /// </summary>
        Task<HealthCheckResult> CheckDatabaseAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Реализация health check поверх Npgsql — прямой ping базы данных,
    /// без загрузки EF-контекста и без затрат на миграции.
    /// </summary>
    public sealed class DatabaseHealthCheckService : IHealthCheckService
    {
        private readonly string _connectionString;
        private readonly int _timeoutSeconds;

        public DatabaseHealthCheckService(string connectionString, int timeoutSeconds = 5)
        {
            _connectionString = connectionString;
            _timeoutSeconds = timeoutSeconds;
        }

        public async Task<HealthCheckResult> CheckDatabaseAsync(CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using var cmd = new NpgsqlCommand("SELECT 1", conn);
                cmd.CommandTimeout = _timeoutSeconds;
                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                sw.Stop();
                var ok = result is int and 1;
                return new HealthCheckResult(
                    ok,
                    "Database",
                    ok ? "PostgreSQL отвечает" : "Неожиданный ответ сервера",
                    sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                return new HealthCheckResult(false, "Database", "Проверка отменена", sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new HealthCheckResult(false, "Database", ex.Message, sw.Elapsed);
            }
        }
    }
}
