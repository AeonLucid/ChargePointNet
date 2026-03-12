using ChargePointNet.Config;
using ChargePointNet.Core;
using ChargePointNet.Core.Interfaces;
using ChargePointNet.Services;
using ChargePointNet.Services.Auth;
using ChargePointNet.Services.OpenApi;
using Scalar.AspNetCore;
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
    builder.Services.AddHostedService<InitializationService>();
    builder.Services.AddHostedService<DeviceRegistrationService>();
    builder.Services.AddSingleton<EVManager>();
    builder.Services.AddSingleton<InMemoryAuthService>();
    builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<InMemoryAuthService>());
    builder.Services.AddControllers();
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<OpenApiDocumentTransformer>();
        // options.AddOperationTransformer<OpenApiOperationResponseTransformer>();
    });
    
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.Title = "ChargePointNet API Docs";
        options.ExpandAllResponses();
        options.ExpandAllTags();
        options.ExpandAllModelSections();
        options.PreserveSchemaPropertyOrder();
        options.HideModels();
        options.DisableTelemetry();
        options.DisableAgent();
    });
    
    app.MapControllers();
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