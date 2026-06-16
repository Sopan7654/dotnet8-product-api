using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Infrastructure.Logging;

/// <summary>
/// Centralised Serilog configuration for the ProductApi application.
/// Configures Console and rolling File sinks with structured logging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Creates the initial bootstrap logger used before the host is built.
    /// </summary>
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures the full Serilog pipeline after the host configuration is available.
    /// Reads settings from appsettings.json and adds Console + rolling File sinks.
    /// </summary>
    public static Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> Configure()
    {
        return (context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ProductApi")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/productapi-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

            // In development, show EF Core SQL queries
            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore.Database.Command",
                    LogEventLevel.Information);
            }
        };
    }
}
