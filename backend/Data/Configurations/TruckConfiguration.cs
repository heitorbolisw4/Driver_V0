using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Plate)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(t => t.Plate)
            .IsUnique();

        builder.Property(t => t.Hodometer)
            .IsRequired();

        // Rota guarda seus próprios dados (km, datas); perder o vínculo com o
        // caminhão não invalida o histórico da rota, então só desvincula.
        builder.HasMany(t => t.Routes)
            .WithOne(r => r.Truck)
            .HasForeignKey(r => r.TruckId)
            .OnDelete(DeleteBehavior.SetNull);

        // Leitura de hodômetro só existe em função do caminhão; sem ele o
        // registro não tem mais sentido.
        builder.HasMany(t => t.OdometerReadings)
            .WithOne(o => o.Truck)
            .HasForeignKey(o => o.TruckId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
