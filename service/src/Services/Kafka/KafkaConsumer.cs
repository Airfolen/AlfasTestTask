using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlfasTestTask.Abstractions;
using AlfasTestTask.Services.Kafka.Models;
using AlfasTestTask.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AlfasTestTask.Services.Kafka;

/// <inheritdoc cref="AlfasTestTask.Abstractions.IKafkaConsumer" />
public sealed class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly AppSettings _settings;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IAdminClient _adminClient;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public KafkaConsumer(IOptions<AppSettings> settings, ILogger<KafkaConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.Value.KafkaBootstrapServers,
            GroupId = "prime-consumer",
            AutoOffsetReset = AutoOffsetReset.Latest,
        };
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = settings.Value.KafkaBootstrapServers
        };
        _adminClient = new AdminClientBuilder(adminConfig).Build();
        _consumer.Subscribe(_settings.KafkaTopic);
    }

    /// <inheritdoc />
    public async Task<ulong?> GetLastPrimeAsync()
    {
        try
        {
            SetTopicOffsetOnLastMessage();

            var consumeResult = await Task.Run(() => _consumer.Consume(_timeout));

            if (consumeResult is not null)
            {
                var lastMessage = consumeResult.Message.Value;
                var number = JsonConvert.DeserializeObject<PrimeNumberDto>(lastMessage);
                return number?.PrimeNumber;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unknown error in kafka consumer");
        }

        return null;
    }

    /// <summary>
    /// Установка оффсета топика на последнее сообщение
    /// </summary>
    private void SetTopicOffsetOnLastMessage()
    {
        var metadata = _adminClient.GetMetadata(_settings.KafkaTopic, TimeSpan.FromSeconds(5));
        var topicMetadata = metadata.Topics.SingleOrDefault(t => t.Topic == _settings.KafkaTopic)
                            ?? throw new KafkaException(ErrorCode.UnknownTopicOrPart);

        var partitions = topicMetadata.Partitions.Select(p => p.PartitionId).ToList();
        var topicPartitionOffsets = new List<TopicPartitionOffset>();

        foreach (var partitionId in partitions)
        {
            var partition = new TopicPartition(_settings.KafkaTopic, new Partition(partitionId));
            var watermarkOffsets = _consumer.QueryWatermarkOffsets(partition, _timeout);

            // Получаем последний оффсет в партиции
            var lastOffset = watermarkOffsets.High - 1;
            // Если сообщений нет, пропускаем партицию
            if (lastOffset < 0) continue;

            topicPartitionOffsets.Add(new TopicPartitionOffset(partition, lastOffset));
        }

        if (topicPartitionOffsets.Any() is false)
        {
            _logger.LogInformation("No messages in the topic {Topic}", _settings.KafkaTopic);
            return;
        }

        // Назначаем партиции и сдвигаем offset
        _consumer.Assign(topicPartitionOffsets);
        foreach (var offset in topicPartitionOffsets)
        {
            _consumer.Seek(offset);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        _adminClient.Dispose();
    }
}