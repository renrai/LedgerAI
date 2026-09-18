namespace LedgerAI.Application.DTOs;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(Guid UserId, string Name, string Email, string AccessToken, DateTimeOffset ExpiresAt);

public sealed record UserDto(Guid Id, string Name, string Email, DateTimeOffset CreatedAt);
