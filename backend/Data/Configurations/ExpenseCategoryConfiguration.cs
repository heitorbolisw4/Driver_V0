using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.HasKey(ec => ec.Id);

        builder.Property(ec => ec.Name)
            .IsRequired()
            .HasMaxLength(80);

        // RN-18: categorias são específicas de cada motorista, não compartilhadas
        // entre contas — por isso a unicidade de nome é por DriverId, não global.
        builder.HasIndex(ec => new { ec.DriverId, ec.Name })
            .IsUnique();

        // Sem a categoria não há como manter o vínculo do gasto; remover a
        // categoria exige antes reatribuir ou remover os gastos associados.
        builder.HasMany(ec => ec.Expenses)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
