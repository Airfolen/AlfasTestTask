namespace AlfasTestTask.Abstractions;

/// <summary>
/// Сервис для отправки простых чисел в Kafka
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Отпраляюет просто число с метоинформацией в Kafka
    /// </summary>
    Task SendPrimeAsync(ulong prime);
}