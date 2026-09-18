namespace LedgerAI.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
