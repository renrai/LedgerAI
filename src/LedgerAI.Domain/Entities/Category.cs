using LedgerAI.Domain.Common;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Exceptions;

namespace LedgerAI.Domain.Entities;

public class Category : Entity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TransactionType Kind { get; private set; }
    public string? Icon { get; private set; }

    /// <summary>Categorias padrão criadas no cadastro; não podem ser removidas.</summary>
    public bool IsSystem { get; private set; }

    private Category() { } // EF Core

    public static Category Create(Guid userId, string name, TransactionType kind, string? icon = null, bool isSystem = false)
    {
        if (userId == Guid.Empty) throw new DomainException("Categoria precisa de um usuário.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category { UserId = userId, Name = name.Trim(), Kind = kind, Icon = icon, IsSystem = isSystem };
    }

    public void Update(string name, string? icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Icon = icon;
        Touch();
    }

    public void EnsureDeletable()
    {
        if (IsSystem) throw new DomainException("Categorias do sistema não podem ser removidas.");
    }

    /// <summary>Conjunto inicial de categorias criado para cada novo usuário.</summary>
    public static IReadOnlyList<Category> CreateDefaultsFor(Guid userId) =>
    [
        Create(userId, "Salário", TransactionType.Income, "money", isSystem: true),
        Create(userId, "Investimentos", TransactionType.Income, "trending-up", isSystem: true),
        Create(userId, "Outras Receitas", TransactionType.Income, "plus", isSystem: true),
        Create(userId, "Alimentação", TransactionType.Expense, "utensils", isSystem: true),
        Create(userId, "Moradia", TransactionType.Expense, "home", isSystem: true),
        Create(userId, "Transporte", TransactionType.Expense, "car", isSystem: true),
        Create(userId, "Saúde", TransactionType.Expense, "heart-pulse", isSystem: true),
        Create(userId, "Educação", TransactionType.Expense, "book", isSystem: true),
        Create(userId, "Lazer", TransactionType.Expense, "gamepad", isSystem: true),
        Create(userId, "Compras", TransactionType.Expense, "shopping-bag", isSystem: true),
        Create(userId, "Assinaturas", TransactionType.Expense, "tv", isSystem: true),
        Create(userId, "Outras Despesas", TransactionType.Expense, "minus", isSystem: true)
    ];
}
