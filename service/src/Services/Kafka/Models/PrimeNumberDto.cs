using Newtonsoft.Json;

namespace AlfasTestTask.Services.Kafka.Models;

/// <summary>
/// Модель данных для передачи простого числа
/// </summary>
public sealed record PrimeNumberDto(string Nickname, ulong PrimeNumber)
{
    /// <summary>
    /// Время генерации простого числа
    /// </summary>
    [JsonProperty("generated_at")]
    public string GeneratedAt { get; set; } = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [JsonProperty("nickname")]
    public string Nickname { get; set; } = Nickname;

    /// <summary>
    /// Число
    /// </summary>
    [JsonProperty("prime_number")]
    public ulong PrimeNumber { get; set; } = PrimeNumber;
}