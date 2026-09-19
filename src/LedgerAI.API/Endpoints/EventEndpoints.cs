using System.Net.ServerSentEvents;
using LedgerAI.API.Infrastructure;
using LedgerAI.Application.Interfaces;

namespace LedgerAI.API.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/events").WithTags("Events").RequireAuthorization();

        // .NET 10: TypedResults.ServerSentEvents transforma um IAsyncEnumerable em um stream text/event-stream,
        // com serialização JSON automática de cada item e keep-alive gerenciado pelo framework.
        group.MapGet("/budget-alerts", (BudgetAlertBroker broker, ICurrentUser user, CancellationToken ct) =>
        {
            var stream = broker.SubscribeAsync(user.UserId, ct)
                .Select(alert => new SseItem<BudgetAlert>(alert, eventType: alert.Level == BudgetAlertLevel.Exceeded ? "budget-exceeded" : "budget-warning")
                {
                    EventId = alert.RaisedAt.ToUnixTimeMilliseconds().ToString()
                });

            return TypedResults.ServerSentEvents(stream);
        })
        .WithSummary("Stream (Server-Sent Events) de alertas de orçamento em tempo real: 80% e 100% do limite.")
        .Produces(StatusCodes.Status200OK, contentType: "text/event-stream");

        return group;
    }
}
