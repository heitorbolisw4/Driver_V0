using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        // RN-20: valor deve ser >= 0.
        builder.Property(e => e.Value)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // FK para Category já é configurada em ExpenseCategoryConfiguration
        // (lado "1", Restrict).

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Expense_Value_NonNegative", "\"Value\" >= 0"));
    }
}
