using ChargePointNet.Config;
using ChargePointNet.Core;
using ChargePointNet.Core.Interfaces;
using ChargePointNet.Services;
using ChargePointNet.Services.Auth;
using ChargePointNet.Services.OpenApi;
using ChargePointNet.Services.Sessions;
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

    builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection(AuthConfig.Section));
    builder.Services.Configure<DevicesConfig>(builder.Configuration.GetSection(DevicesConfig.Section));
    builder.Services.AddHostedService<InitializationService>();
    builder.Services.AddHostedService<DeviceRegistrationService>();
    builder.Services.AddSingleton<EVManager>();

    var auth = builder.Configuration.GetSection(AuthConfig.Section).Get<AuthConfig>();
    if (auth != null && auth.Automatic)
    {
        Log.Information("Using automatic authorization, using configured allowed list");
        
        builder.Services.AddSingleton<IAuthService, AuthServiceAutomatic>();
    }
    else
    {
        Log.Information("Using manual authorization, pending requests must be approved or denied via the API");
        
        builder.Services.AddSingleton<IAuthService, AuthServiceInMemory>();
    }
    
    builder.Services.AddSingleton<IAuthRepository>(sp => sp.GetRequiredService<IAuthService>());
    
    builder.Services.AddSingleton<ISessionService, SessionServiceInMemory>();
    builder.Services.AddSingleton<ISessionRepository>(sp => sp.GetRequiredService<ISessionService>());
    
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