using LedgerAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerAI.Infrastructure.Data.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.Property(t => t.OccurredAt).HasColumnType("date");
        builder.Property(t => t.CategorizationSource).HasConversion<string>().HasMaxLength(10);

        // EF Core 8+: complex type — as duas propriedades de Money viram colunas da própria tabela,
        // sem tabela ou chave separada (diferente de owned types).
        builder.ComplexProperty(t => t.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.UserId, t.OccurredAt });
        builder.HasIndex(t => new { t.UserId, t.CategoryId });
        builder.HasIndex(t => t.AccountId);

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsCategorized);
        builder.Ignore(t => t.SignedAmount);
    }
}
