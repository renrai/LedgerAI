using LedgerAI.API.Endpoints;
using LedgerAI.API.Extensions;
using LedgerAI.API.Infrastructure;
using LedgerAI.Application;
using LedgerAI.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/ledgerai-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithTitle("LedgerAI")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");

    var api = app.MapGroup("/api/v1");
    api.MapAuthEndpoints();
    api.MapAccountEndpoints();
    api.MapCategoryEndpoints();
    api.MapTransactionEndpoints();
    api.MapBudgetEndpoints();
    api.MapDashboardEndpoints();
    api.MapEventEndpoints();

    await app.MigrateDatabaseAsync();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}

// Torna a classe Program visível para o WebApplicationFactory dos testes de integração.
public partial class Program;
