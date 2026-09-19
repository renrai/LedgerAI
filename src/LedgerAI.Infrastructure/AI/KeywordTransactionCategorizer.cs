using System.Globalization;
using System.Text;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Enums;

namespace LedgerAI.Infrastructure.AI;

/// <summary>
/// Classificador determinístico por palavras-chave. Serve como padrão quando nenhum provedor de IA
/// está configurado e como fallback quando o LLM falha. Marca as sugestões como <see cref="CategorizationSource.Rule"/>.
/// </summary>
public sealed class KeywordTransactionCategorizer : ITransactionCategorizer
{
    private const double RuleConfidence = 0.75;

    // Palavra-chave (normalizada, sem acento) → nome da categoria padrão.
    private static readonly (string Keyword, string Category)[] Rules =
    [
        ("uber", "Transporte"), ("99app", "Transporte"), ("99 pop", "Transporte"), ("taxi", "Transporte"),
        ("posto", "Transporte"), ("combustivel", "Transporte"), ("gasolina", "Transporte"), ("estacionamento", "Transporte"),
        ("pedagio", "Transporte"), ("metro", "Transporte"), ("onibus", "Transporte"), ("bilhete unico", "Transporte"),

        ("ifood", "Alimentação"), ("rappi", "Alimentação"), ("restaurante", "Alimentação"), ("lanchonete", "Alimentação"),
        ("padaria", "Alimentação"), ("mercado", "Alimentação"), ("supermercado", "Alimentação"), ("carrefour", "Alimentação"),
        ("pao de acucar", "Alimentação"), ("assai", "Alimentação"), ("atacadao", "Alimentação"), ("burger", "Alimentação"),
        ("pizza", "Alimentação"), ("cafe", "Alimentação"), ("starbucks", "Alimentação"), ("mc donalds", "Alimentação"), ("mcdonalds", "Alimentação"),

        ("aluguel", "Moradia"), ("condominio", "Moradia"), ("iptu", "Moradia"), ("energia", "Moradia"), ("enel", "Moradia"),
        ("luz", "Moradia"), ("agua", "Moradia"), ("sabesp", "Moradia"), ("gas", "Moradia"), ("internet", "Moradia"), ("vivo fibra", "Moradia"),

        ("farmacia", "Saúde"), ("drogaria", "Saúde"), ("drogasil", "Saúde"), ("raia", "Saúde"), ("hospital", "Saúde"),
        ("clinica", "Saúde"), ("consulta", "Saúde"), ("plano de saude", "Saúde"), ("unimed", "Saúde"), ("academia", "Saúde"), ("smart fit", "Saúde"),

        ("curso", "Educação"), ("udemy", "Educação"), ("alura", "Educação"), ("faculdade", "Educação"), ("escola", "Educação"), ("livraria", "Educação"),

        ("netflix", "Assinaturas"), ("spotify", "Assinaturas"), ("amazon prime", "Assinaturas"), ("disney", "Assinaturas"),
        ("hbo", "Assinaturas"), ("youtube premium", "Assinaturas"), ("icloud", "Assinaturas"), ("google one", "Assinaturas"),
        ("github", "Assinaturas"), ("chatgpt", "Assinaturas"), ("claude", "Assinaturas"),

        ("cinema", "Lazer"), ("ingresso", "Lazer"), ("show", "Lazer"), ("steam", "Lazer"), ("playstation", "Lazer"),
        ("xbox", "Lazer"), ("bar ", "Lazer"), ("viagem", "Lazer"), ("hotel", "Lazer"), ("airbnb", "Lazer"),

        ("amazon", "Compras"), ("mercado livre", "Compras"), ("shopee", "Compras"), ("magazine", "Compras"),
        ("americanas", "Compras"), ("shein", "Compras"), ("aliexpress", "Compras"), ("renner", "Compras"), ("zara", "Compras"),

        ("salario", "Salário"), ("pagamento salario", "Salário"), ("folha", "Salário"), ("pro-labore", "Salário"), ("pro labore", "Salário"),
        ("dividendo", "Investimentos"), ("rendimento", "Investimentos"), ("juros", "Investimentos"), ("resgate", "Investimentos"), ("cdb", "Investimentos"),
        ("pix recebido", "Outras Receitas"), ("reembolso", "Outras Receitas"), ("cashback", "Outras Receitas")
    ];

    public Task<IReadOnlyList<CategorizationSuggestion>> SuggestAsync(
        IReadOnlyList<CategorizationRequest> transactions,
        IReadOnlyList<CategorizationCandidate> categories,
        CancellationToken ct = default)
    {
        var byName = categories
            .GroupBy(c => (Normalize(c.Name), c.Kind))
            .ToDictionary(g => g.Key, g => g.First());

        IReadOnlyList<CategorizationSuggestion> result =
        [
            .. transactions.Select(t =>
            {
                var description = Normalize(t.Description);

                foreach (var (keyword, categoryName) in Rules)
                {
                    if (!description.Contains(keyword, StringComparison.Ordinal)) continue;
                    if (!byName.TryGetValue((Normalize(categoryName), t.Type), out var category)) continue;

                    return new CategorizationSuggestion(
                        t.TransactionId, category.Id, RuleConfidence, $"Palavra-chave: {keyword}", CategorizationSource.Rule);
                }

                return new CategorizationSuggestion(t.TransactionId, null, 0, "Nenhuma regra correspondente", CategorizationSource.Rule);
            })
        ];

        return Task.FromResult(result);
    }

    /// <summary>Minúsculas, sem acentos e com espaços colapsados — para comparação tolerante.</summary>
    internal static string Normalize(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(ch));
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
