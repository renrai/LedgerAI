using System.Globalization;
using System.Text;
using System.Text.Json;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Enums;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LedgerAI.Infrastructure.AI;

/// <summary>
/// Classifica transações com um LLM através da abstração <see cref="IChatClient"/> do
/// <c>Microsoft.Extensions.AI</c>. Usa saída estruturada (JSON Schema) para garantir o formato.
/// Se o modelo falhar, cai para o classificador por palavras-chave.
/// </summary>
public sealed class LlmTransactionCategorizer(
    IChatClient chatClient,
    KeywordTransactionCategorizer fallback,
    ILogger<LlmTransactionCategorizer> logger) : ITransactionCategorizer
{
    private const string SystemPrompt = """
        Você é um assistente de finanças pessoais brasileiro. Sua tarefa é classificar transações
        bancárias em UMA das categorias fornecidas, escolhendo sempre pelo id.

        Regras:
        - Só use categorias cujo "kind" seja igual ao "type" da transação (Income com Income, Expense com Expense).
        - Se não tiver certeza razoável, devolva categoryId null e confidence baixa.
        - confidence é um número entre 0 e 1.
        - reason é uma justificativa curta (máx. 12 palavras), em português.
        - Responda apenas com o JSON pedido.
        """;

    public async Task<IReadOnlyList<CategorizationSuggestion>> SuggestAsync(
        IReadOnlyList<CategorizationRequest> transactions,
        IReadOnlyList<CategorizationCandidate> categories,
        CancellationToken ct = default)
    {
        if (transactions.Count == 0) return [];

        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt),
                new(ChatRole.User, BuildUserPrompt(transactions, categories))
            };

            var response = await chatClient.GetResponseAsync<LlmBatchResult>(
                messages,
                new ChatOptions { Temperature = 0 },
                cancellationToken: ct);

            if (!response.TryGetResult(out var result) || result?.Items is null)
            {
                logger.LogWarning("LLM devolveu resposta inválida; usando fallback por palavras-chave.");
                return await fallback.SuggestAsync(transactions, categories, ct);
            }

            var validIds = categories.Select(c => c.Id).ToHashSet();
            var byTx = result.Items
                .Where(i => i.TransactionId != Guid.Empty)
                .GroupBy(i => i.TransactionId)
                .ToDictionary(g => g.Key, g => g.First());

            return
            [
                .. transactions.Select(t =>
                {
                    if (!byTx.TryGetValue(t.TransactionId, out var item) || item.CategoryId is not { } id || !validIds.Contains(id))
                        return new CategorizationSuggestion(t.TransactionId, null, 0, item?.Reason);

                    return new CategorizationSuggestion(t.TransactionId, id, Math.Clamp(item.Confidence, 0, 1), item.Reason);
                })
            ];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Falha ao chamar o LLM; usando fallback por palavras-chave.");
            return await fallback.SuggestAsync(transactions, categories, ct);
        }
    }

    private static string BuildUserPrompt(IReadOnlyList<CategorizationRequest> transactions, IReadOnlyList<CategorizationCandidate> categories)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Categorias disponíveis:");
        foreach (var c in categories)
            sb.AppendLine(CultureInfo.InvariantCulture, $"- id={c.Id} name=\"{c.Name}\" kind={c.Kind}");

        sb.AppendLine();
        sb.AppendLine("Transações para classificar:");
        foreach (var t in transactions)
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"- transactionId={t.TransactionId} type={t.Type} amount={t.Amount:0.00} date={t.OccurredAt:yyyy-MM-dd} description=\"{t.Description}\"");

        sb.AppendLine();
        sb.AppendLine("Devolva um objeto JSON no formato: {\"items\":[{\"transactionId\":\"...\",\"categoryId\":\"...\"|null,\"confidence\":0.0,\"reason\":\"...\"}]}");

        return sb.ToString();
    }

    // Tipos usados pela saída estruturada. Precisam ser públicos para o gerador de JSON Schema.
    public sealed class LlmBatchResult
    {
        public List<LlmItem> Items { get; set; } = [];
    }

    public sealed class LlmItem
    {
        public Guid TransactionId { get; set; }
        public Guid? CategoryId { get; set; }
        public double Confidence { get; set; }
        public string? Reason { get; set; }
    }
}
