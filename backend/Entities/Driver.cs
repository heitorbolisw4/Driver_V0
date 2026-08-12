namespace backend.Entities;

public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Armazenado como hash (BCrypt), nunca em texto puro.
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Route> Routes { get; set; } = [];
    public ICollection<TruckLoad> TruckLoads { get; set; } = [];
    public ICollection<Truck> Trucks { get; set; } = [];
    public ICollection<ExpenseCategory> ExpenseCategories { get; set; } = [];
}
