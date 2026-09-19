using System.ComponentModel.DataAnnotations;

namespace LedgerAI.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32, ErrorMessage = "Jwt:Key precisa ter pelo menos 32 caracteres.")]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "LedgerAI";

    [Required]
    public string Audience { get; set; } = "LedgerAI";

    [Range(1, 24 * 30)]
    public int ExpirationHours { get; set; } = 8;
}
