using backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(180);

        builder.HasIndex(d => d.Email)
            .IsUnique();

        // Hash gerado pelo BCrypt tem sempre 60 caracteres.
        builder.Property(d => d.Password)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Histórico de rotas/cargas não deve sumir se o driver for removido.
        builder.HasMany(d => d.Routes)
            .WithOne(r => r.Driver)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.TruckLoads)
            .WithOne(t => t.Driver)
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
