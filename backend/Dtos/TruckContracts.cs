namespace backend.Dtos;

public record CreateTruckRequest(string Plate, float Hodometer);

// RN-07: hodômetro não é editável diretamente pelo motorista, só via
// encerramento de rota — por isso não faz parte deste contrato.
public record UpdateTruckRequest(string Plate);

public record TruckResponse(int Id, string Plate, float Hodometer);
