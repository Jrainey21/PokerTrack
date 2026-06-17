using PokerTrack.Worker;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();


    var isDevelopment = builder.Environment.EnvironmentName == "Development";
    builder.Services.AddSerilog((services, loggerConfig) =>
    {
        loggerConfig
      .Enrich.FromLogContext()
      .Enrich.WithMachineName()
      .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);

        if (isDevelopment)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }
        // Always send to Seq regardless of environment
        loggerConfig.WriteTo.Seq("http://localhost:5341");
    });

    builder.Services.AddHttpClient();
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}