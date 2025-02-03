namespace AlfasTestTask.Abstractions;

/// <summary>
/// Сервис для чтения последнего отправленного простого числа из Kafka
/// </summary>
public interface IKafkaConsumer
{
    /// <summary>
    /// Получает последнее отправленное простое число из Kafka
    /// </summary>
    /// <returns>Последнее простое число или null</returns>
    Task<ulong?> GetLastPrimeAsync();
}