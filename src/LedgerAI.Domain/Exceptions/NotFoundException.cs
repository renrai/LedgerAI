namespace LedgerAI.Domain.Exceptions;

/// <summary>Recurso não encontrado. Mapeada para HTTP 404 na API.</summary>
public class NotFoundException(string entity, object key)
    : Exception($"{entity} com identificador '{key}' não foi encontrado(a).");
