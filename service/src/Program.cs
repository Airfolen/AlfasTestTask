using AlfasTestTask.Abstractions;
using AlfasTestTask.Services.Kafka;
using AlfasTestTask.Services.Workers;
using AlfasTestTask.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
builder.Services.AddHostedService<PrimeNumberGenerator>();

var app = builder.Build();

app.Run();