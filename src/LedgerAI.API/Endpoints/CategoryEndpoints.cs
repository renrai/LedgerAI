using LedgerAI.API.Infrastructure;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LedgerAI.API.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/categories").WithTags("Categories").RequireAuthorization();

        group.MapGet("/", async Task<Ok<IReadOnlyList<CategoryDto>>> (CategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.ListAsync(ct)))
        .WithSummary("Lista as categorias do usuário (padrão + personalizadas).");

        group.MapGet("/{id:guid}", async Task<Ok<CategoryDto>> (Guid id, CategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAsync(id, ct)))
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<CategoryDto>> (CreateCategoryRequest request, CategoryService service, CancellationToken ct) =>
        {
            var category = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/v1/categories/{category.Id}", category);
        })
        .WithValidation<CreateCategoryRequest>()
        .WithSummary("Cria uma categoria personalizada.");

        group.MapPut("/{id:guid}", async Task<Ok<CategoryDto>> (Guid id, UpdateCategoryRequest request, CategoryService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UpdateAsync(id, request, ct)))
        .WithValidation<UpdateCategoryRequest>();

        group.MapDelete("/{id:guid}", async Task<NoContent> (Guid id, CategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithSummary("Remove uma categoria personalizada. Categorias do sistema não podem ser removidas.")
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
