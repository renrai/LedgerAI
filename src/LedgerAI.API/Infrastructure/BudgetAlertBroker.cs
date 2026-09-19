using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using LedgerAI.Application.Interfaces;

namespace LedgerAI.API.Infrastructure;

/// <summary>
/// Broker em memória de alertas de orçamento. Cada assinante (conexão SSE) recebe um
/// <see cref="Channel{T}"/> próprio; a publicação faz fan-out para todos os canais do usuário.
/// Em um cenário multi-instância, troque por Redis Pub/Sub ou Azure SignalR.
/// </summary>
public sealed class BudgetAlertBroker : IBudgetAlertPublisher
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<BudgetAlert>>> _subscribers = new();

    public ValueTask PublishAsync(BudgetAlert alert, CancellationToken ct = default)
    {
        if (!_subscribers.TryGetValue(alert.UserId, out var channels))
            return ValueTask.CompletedTask;

        foreach (var channel in channels.Values)
            channel.Writer.TryWrite(alert); // canal bounded com DropOldest: nunca bloqueia o publicador

        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<BudgetAlert> SubscribeAsync(Guid userId, [EnumeratorCancellation] CancellationToken ct)
    {
        var subscriptionId = Guid.CreateVersion7();
        var channel = Channel.CreateBounded<BudgetAlert>(new BoundedChannelOptions(32)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

        var channels = _subscribers.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, Channel<BudgetAlert>>());
        channels[subscriptionId] = channel;

        try
        {
            await foreach (var alert in channel.Reader.ReadAllAsync(ct))
                yield return alert;
        }
        finally
        {
            channels.TryRemove(subscriptionId, out _);
            if (channels.IsEmpty)
                _subscribers.TryRemove(userId, out _);
        }
    }

    public int SubscriberCount(Guid userId) =>
        _subscribers.TryGetValue(userId, out var channels) ? channels.Count : 0;
}
