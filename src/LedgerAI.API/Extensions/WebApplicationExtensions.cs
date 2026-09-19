using LedgerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes na inicialização quando <c>Database:MigrateOnStartup</c> for true
    /// (padrão em Development e no docker-compose). Em produção prefira rodar migrations no pipeline.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Database:MigrateOnStartup", app.Environment.IsDevelopment()))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Banco de dados já está atualizado.");
            return;
        }

        logger.LogInformation("Aplicando {Count} migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
        await db.Database.MigrateAsync();
    }
}
