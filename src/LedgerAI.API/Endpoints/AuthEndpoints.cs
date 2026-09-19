using LedgerAI.API.Infrastructure;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LedgerAI.API.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async Task<Created<AuthResponse>> (RegisterRequest request, AuthService auth, CancellationToken ct) =>
        {
            var response = await auth.RegisterAsync(request, ct);
            return TypedResults.Created("/api/v1/auth/me", response);
        })
        .WithValidation<RegisterRequest>()
        .WithSummary("Cria uma conta de usuário e devolve o token de acesso.")
        .AllowAnonymous();

        group.MapPost("/login", async Task<Ok<AuthResponse>> (LoginRequest request, AuthService auth, CancellationToken ct) =>
            TypedResults.Ok(await auth.LoginAsync(request, ct)))
        .WithValidation<LoginRequest>()
        .WithSummary("Autentica e devolve um JWT.")
        .AllowAnonymous();

        group.MapGet("/me", async Task<Ok<UserDto>> (ICurrentUser user, AuthService auth, CancellationToken ct) =>
            TypedResults.Ok(await auth.GetProfileAsync(user.UserId, ct)))
        .WithSummary("Perfil do usuário autenticado.")
        .RequireAuthorization();

        return group;
    }
}
