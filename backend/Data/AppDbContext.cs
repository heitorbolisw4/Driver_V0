using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<TruckLoad> TruckLoads => Set<TruckLoad>();
    public DbSet<OdometerReading> OdometerReadings => Set<OdometerReading>();


}
