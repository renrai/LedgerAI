using LedgerAI.API.Infrastructure;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;

namespace LedgerAI.API.Endpoints;

public static class BudgetEndpoints
{
    public static RouteGroupBuilder MapBudgetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/budgets").WithTags("Budgets").RequireAuthorization();

        group.MapGet("/", async Task<Ok<IReadOnlyList<BudgetDto>>> (BudgetService service, string? period = null, CancellationToken ct = default) =>
            TypedResults.Ok(await service.ListAsync(period, ct)))
        .WithSummary("Lista os orçamentos de uma competência (YYYY-MM; padrão = mês atual) com o gasto realizado.");

        group.MapPut("/", async Task<Ok<BudgetDto>> (
            UpsertBudgetRequest request, BudgetService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            var budget = await service.UpsertAsync(request, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.Ok(budget);
        })
        .WithValidation<UpsertBudgetRequest>()
        .WithSummary("Cria ou atualiza o limite de uma categoria na competência.");

        group.MapDelete("/{id:guid}", async Task<NoContent> (
            Guid id, BudgetService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.NoContent();
        });

        return group;
    }
}
