using API.Extensions;
using Asp.Versioning;
using Infrastructure.Logging;
using Serilog;

SerilogConfiguration.ConfigureBootstrapLogger();

try
{
    Log.Information("Starting ProductApi...");

    var builder = WebApplication.CreateBuilder(args);

  
    builder.Host.UseSerilog(SerilogConfiguration.Configure());


    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddResponseCompression(opts => opts.EnableForHttps = true);
    builder.Services.AddHealthChecks();

    builder.Services
        .AddCorsPolicy()
        .AddJwtAuthentication(builder.Configuration)
        .AddAutoMapperAndValidation()
        .AddApplicationServices()
        .AddInfrastructureServices(builder.Configuration)
        .AddSwaggerDocumentation();

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    var app = builder.Build();

    await app.MigrateAndSeedDatabaseAsync();
    app.UseSecurityHeaders();
    app.UseResponseCompression();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
            c.RoutePrefix = "swagger";
            c.DefaultModelsExpandDepth(-1); // Hides the Schemas section
        });
    }

    app.UseCors("AllowAll");
    app.UseCustomMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ProductApi terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for WebApplicationFactory in integration tests
public partial class Program { }
