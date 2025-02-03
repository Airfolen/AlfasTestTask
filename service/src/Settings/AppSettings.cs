namespace AlfasTestTask.Settings;

/// <summary>
/// Конфигурация приложения
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Адрес Kafka-брокера
    /// </summary>
    public string KafkaBootstrapServers { get; set; } = "kafka:9092";

    /// <summary>
    /// Топик Kafka, в который отправляются числа
    /// </summary>
    public string KafkaTopic { get; set; } = "primes";

    /// <summary>
    /// Никнейм пользователя для записи в ClickHouse
    /// </summary>
    public string Nickname { get; set; } = "alfn";
}