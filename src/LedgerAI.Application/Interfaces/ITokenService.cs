using LedgerAI.Domain.Entities;

namespace LedgerAI.Application.Interfaces;

public sealed record AuthToken(string AccessToken, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AuthToken Generate(User user);
}
