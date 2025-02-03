using AlfasTestTask.Abstractions;

namespace AlfasTestTask.Services.Workers;

/// <summary>
/// Сервис генерации простых чисел
/// </summary>
public sealed class PrimeNumberGenerator : BackgroundService
{
    private readonly ILogger<PrimeNumberGenerator> _logger;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IKafkaConsumer _kafkaConsumer;

    private ulong _lastPrime = 1;

    public PrimeNumberGenerator(ILogger<PrimeNumberGenerator> logger, IKafkaProducer kafkaProducer, IKafkaConsumer kafkaConsumer)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _kafkaConsumer = kafkaConsumer;
    }

    /// <summary>
    /// Запуск генерации простых чисел
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Search for the last sent number, if available");

        var lastPrime = await _kafkaConsumer.GetLastPrimeAsync();

        if (lastPrime.HasValue)
        {
            _lastPrime = lastPrime.Value;
            _logger.LogInformation("Last found prime number: {LastPrime}", _lastPrime);
        }
        else
            _logger.LogInformation("The last number is not found, starting with 1");

        _logger.LogInformation("The beginning of number generation...");
        while (stoppingToken.IsCancellationRequested is false)
        {
            // Генерируем 20 простых чисел
            for (ulong i = 0; i < 20; i++)
            {
                _lastPrime = GetNextPrime(_lastPrime);

                await _kafkaProducer.SendPrimeAsync(_lastPrime);

                _logger.LogInformation("The {LastNumber} number has been sent to Kafka", _lastPrime);

                // Задержка t = x % i мс
                await Task.Delay((int) (_lastPrime % (i + 1)), stoppingToken);
            }

            // Ожидаем до начала следующей секунды
            var delay = 1000 - DateTimeOffset.UtcNow.Millisecond;
            await Task.Delay(delay, stoppingToken);
        }
    }

    /// <summary>
    /// Получение следующего простого числа
    /// </summary>
    private static ulong GetNextPrime(ulong start)
    {
        var num = start + 1;
        while (IsPrime(num) is false)
            num++;
        return num;
    }

    /// <summary>
    /// Проверка числа на простоту
    /// </summary>
    private static bool IsPrime(ulong number)
    {
        if (number < 2) return false;
        if (number == 2 || number == 3) return true;
        // Исключаем четные и кратные 3
        if (number % 2 == 0 || number % 3 == 0) return false;

        for (ulong i = 5; i * i <= number; i++)
            if (number % i == 0)
                return false;

        return true;
    }
}