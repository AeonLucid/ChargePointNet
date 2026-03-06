using ChargePointNet.Config;
using ChargePointNet.Core;
using ChargePointNet.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting ChargePointNet");
    
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.Configure<DevicesConfig>(builder.Configuration.GetSection(DevicesConfig.Section));
    builder.Services.AddHostedService<DeviceRegistrationService>();
    builder.Services.AddSingleton<EVManager>();
    
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.MapGet("/", () => "Hello World!");
    app.Run();
} 
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}