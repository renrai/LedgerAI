namespace LedgerAI.Domain.Enums;

/// <summary>Origem da categoria atribuída a uma transação.</summary>
public enum CategorizationSource
{
    None = 0,
    Manual = 1,
    Rule = 2,
    Ai = 3
}
