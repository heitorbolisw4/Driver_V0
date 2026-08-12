using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class OdometerReadingConfiguration : IEntityTypeConfiguration<OdometerReading>
{
    public void Configure(EntityTypeBuilder<OdometerReading> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Value)
            .IsRequired();

        builder.Property(o => o.ReadingDate)
            .IsRequired();

        builder.Property(o => o.Note)
            .HasMaxLength(500);

        // FK para Truck já é configurada em TruckConfiguration (lado "1",
        // com Cascade: sem o caminhão, a leitura não tem mais sentido).
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_OdometerReading_Value_NonNegative", "\"Value\" >= 0"));
    }
}
