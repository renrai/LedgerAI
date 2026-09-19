using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;

namespace LedgerAI.API.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/dashboard").WithTags("Dashboard").RequireAuthorization();

        group.MapGet("/summary", async Task<Ok<MonthlySummaryDto>> (
            DashboardService service, HybridCache cache, ICurrentUser user, string? period = null, CancellationToken ct = default) =>
        {
            var key = $"dashboard:{user.UserId}:{period ?? "current"}";

            // HybridCache resolve o valor uma única vez mesmo sob requisições concorrentes (stampede protection)
            // e permite invalidação por tag quando o usuário lança/edita transações.
            var summary = await cache.GetOrCreateAsync(
                key,
                async token => await service.GetMonthlySummaryAsync(period, token),
                tags: [CacheTags.User(user.UserId)],
                cancellationToken: ct);

            return TypedResults.Ok(summary);
        })
        .WithSummary("Resumo mensal: receitas, despesas, saldo, gastos por categoria e status dos orçamentos.");

        return group;
    }
}
