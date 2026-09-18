namespace LedgerAI.Application.Interfaces;

/// <summary>Usuário autenticado na requisição atual. Implementado na camada de API a partir dos claims do JWT.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}
