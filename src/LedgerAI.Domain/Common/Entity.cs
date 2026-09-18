namespace LedgerAI.Domain.Common;

/// <summary>
/// Base para todas as entidades. Usa GUID v7 (ordenado por tempo) — novo no .NET 9+ —
/// que gera índices B-Tree muito mais eficientes no PostgreSQL do que GUID v4.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
