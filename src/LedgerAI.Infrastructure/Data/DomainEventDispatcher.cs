using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedgerAI.Infrastructure.Data;

/// <summary>
/// Resolve os <see cref="IDomainEventHandler{TEvent}"/> registrados no container para cada evento
/// e os executa em sequência. Falhas em um handler são registradas e não interrompem os demais.
/// </summary>
public sealed class DomainEventDispatcher(IServiceProvider services, ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

            foreach (var handler in services.GetServices(handlerType))
            {
                if (handler is null) continue;

                try
                {
                    await (Task)handleMethod.Invoke(handler, [domainEvent, ct])!;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Handler {Handler} falhou ao processar {Event}", handler.GetType().Name, domainEvent.GetType().Name);
                }
            }
        }
    }
}
