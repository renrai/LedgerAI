using System.ClientModel;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Infrastructure.AI;
using LedgerAI.Infrastructure.Data;
using LedgerAI.Infrastructure.Repositories;
using LedgerAI.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAI;

namespace LedgerAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistência
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "ledger"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();

        // Segurança
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // IA
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddSingleton<KeywordTransactionCategorizer>();

        var ai = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();

        if (ai.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(ai.OpenAI.ApiKey))
        {
            var clientOptions = new OpenAIClientOptions();
            if (!string.IsNullOrWhiteSpace(ai.OpenAI.Endpoint))
                clientOptions.Endpoint = new Uri(ai.OpenAI.Endpoint);

            var openAi = new OpenAIClient(new ApiKeyCredential(ai.OpenAI.ApiKey), clientOptions);

            // Pipeline do Microsoft.Extensions.AI: o cliente OpenAI vira um IChatClient com
            // logging e telemetria (OpenTelemetry) plugados como middlewares.
            services.AddChatClient(openAi.GetChatClient(ai.OpenAI.Model).AsIChatClient())
                .UseOpenTelemetry()
                .UseLogging();

            services.AddScoped<ITransactionCategorizer, LlmTransactionCategorizer>();
        }
        else
        {
            services.AddSingleton<ITransactionCategorizer>(sp => sp.GetRequiredService<KeywordTransactionCategorizer>());
        }

        return services;
    }
}
