using LedgerAI.API.Infrastructure;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LedgerAI.API.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/accounts").WithTags("Accounts").RequireAuthorization();

        group.MapGet("/", async Task<Ok<IReadOnlyList<AccountDto>>> (AccountService service, bool includeArchived = false, CancellationToken ct = default) =>
            TypedResults.Ok(await service.ListAsync(includeArchived, ct)))
        .WithSummary("Lista as contas do usuário com saldo atual.");

        group.MapGet("/{id:guid}", async Task<Ok<AccountDto>> (Guid id, AccountService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)))
        .WithSummary("Detalha uma conta.")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<AccountDto>> (CreateAccountRequest request, AccountService service, CancellationToken ct) =>
        {
            var account = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/v1/accounts/{account.Id}", account);
        })
        .WithValidation<CreateAccountRequest>()
        .WithSummary("Cria uma conta (corrente, poupança, cartão, investimento ou dinheiro).");

        group.MapPut("/{id:guid}", async Task<Ok<AccountDto>> (Guid id, UpdateAccountRequest request, AccountService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateAsync(id, request, ct)))
        .WithValidation<UpdateAccountRequest>()
        .WithSummary("Atualiza nome e tipo da conta.");

        group.MapPost("/{id:guid}/archive", async Task<NoContent> (Guid id, AccountService service, CancellationToken ct) =>
        {
            await service.ArchiveAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithSummary("Arquiva a conta (mantém histórico, bloqueia novos lançamentos).");

        group.MapPost("/{id:guid}/restore", async Task<NoContent> (Guid id, AccountService service, CancellationToken ct) =>
        {
            await service.RestoreAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithSummary("Restaura uma conta arquivada.");

        return group;
    }
}
