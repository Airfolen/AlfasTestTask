-- 1. Создание базы данных
CREATE DATABASE IF NOT EXISTS alfn;

-- 2. Используем созданную БД
USE alfn;

-- 3. Таблица, которая читает данные из Kafka
CREATE TABLE IF NOT EXISTS alfn.primes_kafka
(
    generated_at DateTime,       -- Время генерации числа
    nickname String,           -- Никнейм (alfn)
    prime_number UInt64        -- Простое число
)
ENGINE = Kafka
SETTINGS kafka_broker_list = 'kafka:9092',
         kafka_topic_list = 'primes',
         kafka_group_name = 'clickhouse_group',
         kafka_format = 'JSONEachRow',
         kafka_flush_interval_ms = 1000;

-- 4. Основная таблица с данными primes
CREATE TABLE IF NOT EXISTS alfn.primes
(
    generated_at  DateTime DEFAULT now(), -- Время генерации числа
    nickname      String,                 -- Никнейм (alfn)
    prime_number  UInt64                  -- Простое число
)
ENGINE = MergeTree()
ORDER BY generated_at;

-- 5 Материализованное представление для переноса данных из Kafka в `primes`
CREATE MATERIALIZED VIEW IF NOT EXISTS alfn.primes_mv TO alfn.primes AS
SELECT generated_at,
       nickname,
       prime_number
FROM alfn.primes_kafka;