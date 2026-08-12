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

        // RN-05: a placa deve ser única por motorista, não globalmente.
        builder.HasIndex(t => new { t.DriverId, t.Plate })
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

        // RN-06: hodômetro inicial deve ser >= 0.
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Truck_Hodometer_NonNegative", "\"Hodometer\" >= 0"));
    }
}
