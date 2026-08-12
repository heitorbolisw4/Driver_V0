using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RouteName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Kilometers)
            .IsRequired();

        builder.Property(r => r.StartOdometer)
            .IsRequired();

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.InProcess)
            .IsRequired()
            .HasDefaultValue(true);

        // As FKs para Driver, Truck e TruckLoad já são configuradas a partir
        // do lado "1" de cada relação (DriverConfiguration, TruckConfiguration,
        // TruckLoadConfiguration), então aqui só ficam as regras próprias da Route.

        // RN-19: gasto só existe em função da rota; sem ela o lançamento não
        // tem mais sentido (regra de exclusão em cascata ainda não confirmada
        // com o cliente — ver Seção 8 do documento de regras de negócio).
        builder.HasMany(r => r.Expenses)
            .WithOne(e => e.Route)
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Route_Kilometers_NonNegative", "\"Kilometers\" >= 0");
            t.HasCheckConstraint("CK_Route_KilometersCovered_NonNegative", "\"KilometersCovered\" IS NULL OR \"KilometersCovered\" >= 0");
            t.HasCheckConstraint("CK_Route_StartOdometer_NonNegative", "\"StartOdometer\" >= 0");
            t.HasCheckConstraint("CK_Route_EndDate_AfterStartDate", "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\"");
        });
    }
}
