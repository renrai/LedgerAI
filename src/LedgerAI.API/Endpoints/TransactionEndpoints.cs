using LedgerAI.API.Infrastructure;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;

namespace LedgerAI.API.Endpoints;

public static class TransactionEndpoints
{
    public static RouteGroupBuilder MapTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/transactions").WithTags("Transactions").RequireAuthorization();

        group.MapGet("/", async Task<Ok<PagedResponse<TransactionDto>>> ([AsParameters] TransactionQuery query, TransactionService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SearchAsync(query, ct)))
        .WithSummary("Busca transações com filtros por período, conta, categoria, tipo e texto.");

        group.MapGet("/{id:guid}", async Task<Ok<TransactionDto>> (Guid id, TransactionService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)))
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<TransactionDto>> (
            CreateTransactionRequest request, TransactionService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            var transaction = await service.CreateAsync(request, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.Created($"/api/v1/transactions/{transaction.Id}", transaction);
        })
        .WithValidation<CreateTransactionRequest>()
        .WithSummary("Lança uma transação. Sem categoria informada, a IA categoriza automaticamente.");

        group.MapPut("/{id:guid}", async Task<Ok<TransactionDto>> (
            Guid id, UpdateTransactionRequest request, TransactionService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            var transaction = await service.UpdateAsync(id, request, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.Ok(transaction);
        })
        .WithValidation<UpdateTransactionRequest>();

        group.MapPatch("/{id:guid}/category", async Task<Ok<TransactionDto>> (
            Guid id, CategorizeTransactionRequest request, TransactionService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            var transaction = await service.CategorizeAsync(id, request, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.Ok(transaction);
        })
        .WithValidation<CategorizeTransactionRequest>()
        .WithSummary("Define manualmente a categoria (sobrescreve a sugestão da IA).");

        group.MapDelete("/{id:guid}", async Task<NoContent> (
            Guid id, TransactionService service, HybridCache cache, ICurrentUser user, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.NoContent();
        });

        group.MapPost("/categorize-pending", async Task<Ok<CategorizationResult>> (
            CategorizationService service, HybridCache cache, ICurrentUser user, int limit = 50, CancellationToken ct = default) =>
        {
            var result = await service.CategorizePendingAsync(user.UserId, Math.Clamp(limit, 1, 200), ct);
            await cache.RemoveByTagAsync(CacheTags.User(user.UserId), ct);
            return TypedResults.Ok(result);
        })
        .WithSummary("Roda a categorização automática nas transações ainda sem categoria.");

        return group;
    }
}

public static class CacheTags
{
    public static string User(Guid userId) => $"user:{userId}";
}
