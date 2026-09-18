using System.Net.Mail;

namespace LedgerAI.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !MailAddress.TryCreate(value, out _))
            throw new ArgumentException("E-mail inválido.", nameof(value));

        Value = value.Trim().ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string value) => new(value);

    public override string ToString() => Value;
}
