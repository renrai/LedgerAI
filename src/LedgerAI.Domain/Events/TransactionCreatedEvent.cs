using LedgerAI.Domain.Common;

namespace LedgerAI.Domain.Events;

/// <summary>Disparado ao criar uma transação — usado para acionar a categorização automática.</summary>
public sealed record TransactionCreatedEvent(Guid TransactionId, Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
