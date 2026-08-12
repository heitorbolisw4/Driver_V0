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

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.InProcess)
            .IsRequired()
            .HasDefaultValue(true);

        // As FKs para Driver, Truck e TruckLoad já são configuradas a partir
        // do lado "1" de cada relação (DriverConfiguration, TruckConfiguration,
        // TruckLoadConfiguration), então aqui só ficam as regras próprias da Route.
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Route_Kilometers_NonNegative", "\"Kilometers\" >= 0");
            t.HasCheckConstraint("CK_Route_KilometersCovered_NonNegative", "\"KilometersCovered\" IS NULL OR \"KilometersCovered\" >= 0");
            t.HasCheckConstraint("CK_Route_EndDate_AfterStartDate", "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\"");
        });
    }
}
