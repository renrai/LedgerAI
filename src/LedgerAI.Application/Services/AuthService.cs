using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LedgerAI.Application.Services;

public sealed class AuthService(
    IUserRepository users,
    ICategoryRepository categories,
    IPasswordHasher hasher,
    ITokenService tokens,
    IUnitOfWork uow,
    ILogger<AuthService> logger)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            throw new DomainException("E-mail já cadastrado.");

        var user = User.Create(request.Name, request.Email, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);
        await categories.AddRangeAsync(Category.CreateDefaultsFor(user.Id), ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Novo usuário registrado: {UserId}", user.Id);

        var token = tokens.Generate(user);
        return new AuthResponse(user.Id, user.Name, user.Email, token.AccessToken, token.ExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email, ct);

        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("E-mail ou senha inválidos.");

        var token = tokens.Generate(user);
        return new AuthResponse(user.Id, user.Name, user.Email, token.AccessToken, token.ExpiresAt);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct) ?? throw new NotFoundException(nameof(User), userId);
        return user.ToDto();
    }
}

/// <summary>Credenciais inválidas. Mapeada para HTTP 401 na API.</summary>
public sealed class UnauthorizedException(string message) : Exception(message);
