namespace backend.Entities;

public class Truck
{
    public int Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public float Hodometer { get; set; }

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public ICollection<Route> Routes { get; set; } = [];
    public ICollection<OdometerReading> OdometerReadings { get; set; } = [];
}
