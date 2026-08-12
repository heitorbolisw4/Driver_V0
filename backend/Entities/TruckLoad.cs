namespace backend.Entities;

public class TruckLoad
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public float Weight { get; set; }
    public bool IsShipped { get; set; }

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public ICollection<Route> Routes { get; set; } = [];
}
