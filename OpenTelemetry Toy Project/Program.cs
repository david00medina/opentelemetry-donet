using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry_Toy_Project.Features.Dice.Controllers;
using OpenTelemetry_Toy_Project.Telemetry.Exporting;

var serviceName = "dice-server";
var serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: serviceName,
        serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .AddAspNetCoreInstrumentation()
        .AddProcessor(new SimpleActivityExportProcessor(new IntegrationSpanExporter()))
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(serviceName)
        .AddConsoleExporter());

builder.Logging.ClearProviders()
    .AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion)).IncludeScopes = true;
        options.AddProcessor(new SimpleLogRecordExportProcessor(new IntegrationLogExporter()));
        options.AddConsoleExporter();
    });

builder.Services.AddScoped<IDiceController, DiceController>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
