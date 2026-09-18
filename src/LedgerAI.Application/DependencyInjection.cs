using FluentValidation;
using LedgerAI.Application.EventHandlers;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using LedgerAI.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<AccountService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<CategorizationService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<DashboardService>();

        services.AddScoped<IDomainEventHandler<TransactionCreatedEvent>, TransactionCreatedHandler>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
