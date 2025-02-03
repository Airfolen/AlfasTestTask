using System;
using System.Threading.Tasks;
using AlfasTestTask.Abstractions;
using AlfasTestTask.Services.Kafka.Models;
using AlfasTestTask.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AlfasTestTask.Services.Kafka;

/// <inheritdoc cref="AlfasTestTask.Abstractions.IKafkaProducer" />
public sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly AppSettings _settings;

    public KafkaProducer(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;

        var config = new ProducerConfig {BootstrapServers = settings.Value.KafkaBootstrapServers};
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    /// <inheritdoc />
    public async Task SendPrimeAsync(ulong prime)
    {
        var number = new PrimeNumberDto(_settings.Nickname, prime);
        var jsonMessage = JsonConvert.SerializeObject(number);

        await _producer.ProduceAsync(_settings.KafkaTopic, new Message<Null, string> {Value = jsonMessage});
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _producer.Dispose();
    }
}