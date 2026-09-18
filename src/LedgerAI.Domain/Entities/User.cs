using LedgerAI.Domain.Common;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Domain.Entities;

public class User : AggregateRoot
{
    // C# 14: a palavra-chave `field` permite lógica no acessor sem declarar campo de apoio.
    public string Name
    {
        get => field;
        private set => field = value.Trim();
    } = string.Empty;

    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;

    private User() { } // EF Core

    public static User Create(string name, Email email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User { Name = name, Email = email, PasswordHash = passwordHash };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Touch();
    }

    public void ChangePassword(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        Touch();
    }
}
