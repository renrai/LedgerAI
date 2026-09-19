namespace LedgerAI.Infrastructure.AI;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>"OpenAI" usa um LLM via Microsoft.Extensions.AI; "Keyword" (padrão) usa regras locais.</summary>
    public string Provider { get; set; } = "Keyword";

    public OpenAiOptions OpenAI { get; set; } = new();

    public sealed class OpenAiOptions
    {
        public string? ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4o-mini";

        /// <summary>Endpoint alternativo compatível com a API da OpenAI (ex.: Ollama, Azure OpenAI, Groq).</summary>
        public string? Endpoint { get; set; }
    }
}
