using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class TruckLoadConfiguration : IEntityTypeConfiguration<TruckLoad>
{
    public void Configure(EntityTypeBuilder<TruckLoad> builder)
    {
        builder.HasKey(tl => tl.Id);

        builder.Property(tl => tl.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tl => tl.IsShipped)
            .IsRequired()
            .HasDefaultValue(false);

        // FK para Driver já é configurada em DriverConfiguration (lado "1").
        // Aqui configuramos o lado "1" de TruckLoad -> Route: diferente de
        // Driver, a rota só perde a referência (SetNull) se a carga for
        // removida, já que a rota mantém seus próprios dados históricos.
        builder.HasMany(tl => tl.Routes)
            .WithOne(r => r.TruckLoad)
            .HasForeignKey(r => r.TruckLoadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Diferente de KilometersCovered (que legitimamente começa em 0 até a
        // rota ser encerrada), uma carga sempre existe com peso > 0.
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_TruckLoad_Weight_Positive", "\"Weight\" > 0"));
    }
}
