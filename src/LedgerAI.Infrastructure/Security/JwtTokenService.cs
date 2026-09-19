using System.Security.Claims;
using System.Text;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LedgerAI.Infrastructure.Security;

/// <summary>
/// Emite JWTs com <see cref="JsonWebTokenHandler"/> — a implementação moderna e mais rápida
/// que substitui o antigo <c>JwtSecurityTokenHandler</c>.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    private readonly JwtOptions _options = options.Value;
    private static readonly JsonWebTokenHandler Handler = new();

    public AuthToken Generate(User user)
    {
        var now = clock.GetUtcNow();
        var expires = now.AddHours(_options.ExpirationHours);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            ])
        };

        return new AuthToken(Handler.CreateToken(descriptor), expires);
    }
}
