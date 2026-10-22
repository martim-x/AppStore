using System;

namespace AppStore.Core.Services
{
    /// <summary>
    /// Результат проверки состояния инфраструктуры (health check).
    /// </summary>
    public sealed class HealthCheckResult
    {
        public HealthCheckResult(bool isHealthy, string component, string message, TimeSpan latency)
        {
            IsHealthy = isHealthy;
            Component = component;
            Message = message;
            Latency = latency;
        }

        /// <summary>Признак успешной проверки.</summary>
        public bool IsHealthy { get; }

        /// <summary>Имя проверяемого компонента (например, «Database»).</summary>
        public string Component { get; }

        /// <summary>Человекочитаемое описание результата.</summary>
        public string Message { get; }

        /// <summary>Время отклика компонента.</summary>
        public TimeSpan Latency { get; }

        /// <summary>Ответ компонента в мс — удобно для вывода в UI.</summary>
        public long LatencyMs => (long)Latency.TotalMilliseconds;

        public override string ToString() =>
            $"{Component}: {(IsHealthy ? "OK" : "FAIL")} — {Message} ({LatencyMs} ms)";
    }
}
