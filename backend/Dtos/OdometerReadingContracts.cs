namespace backend.Dtos;

// Value/ReadingDate/TruckId não são editáveis: sustentam Route.KilometersCovered
// e Truck.Hodometer já calculados no encerramento da rota (RN-07/RN-13).
public record UpdateOdometerReadingRequest(string? Note);

public record OdometerReadingResponse(int Id, int TruckId, float Value, DateOnly ReadingDate, string? Note);
