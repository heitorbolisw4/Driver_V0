namespace backend.Entities;

public class Route
{
    public int Id { get; set; }
    public string RouteName { get; set; } = string.Empty;

    // Km acordado com a empresa contratante.
    public float Kilometers { get; set; }

    // Nulo enquanto a rota estiver em andamento; apurado ao encerrar a rota.
    public float? KilometersCovered { get; set; }

    public DateOnly StartDate { get; set; }

    // Nula enquanto a rota estiver em andamento (InProcess = true).
    public DateOnly? EndDate { get; set; }
    public bool InProcess { get; set; } = true;

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public int? TruckLoadId { get; set; }
    public TruckLoad? TruckLoad { get; set; }

    public int? TruckId { get; set; }
    public Truck? Truck { get; set; }
}
