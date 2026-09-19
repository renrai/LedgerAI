using LedgerAI.Domain.Entities;
using LedgerAI.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerAI.Infrastructure.Data.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(b => b.Id);

        // YearMonth persistido como inteiro YYYYMM (ex.: 202609) — compacto e ordenável.
        builder.Property(b => b.Period)
            .HasConversion(p => p.ToInt(), v => YearMonth.FromInt(v))
            .HasColumnName("period");

        builder.ComplexProperty(b => b.Limit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("limit_amount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("limit_currency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne<User>().WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>().WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.UserId, b.CategoryId, b.Period }).IsUnique();

        builder.Ignore(b => b.DomainEvents);
    }
}
