namespace backend.Entities;

public class OdometerReading
{
    public int Id { get; set; }
    public float Value { get; set; }
    public DateOnly ReadingDate { get; set; }
    public string? Note { get; set; }

    public int TruckId { get; set; }
    public Truck Truck { get; set; } = null!;
}
