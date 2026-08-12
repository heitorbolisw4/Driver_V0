namespace backend.Dtos;

// RN-09/10: StartDate e StartOdometer são derivados automaticamente na abertura
// (data atual e Truck.Hodometer), por isso não fazem parte deste contrato.
public record CreateRouteRequest(string RouteName, float Kilometers, int TruckId, int? TruckLoadId);

// RN-14: rota segue editável mesmo concluída. TruckId/StartOdometer não são
// editáveis aqui — trocar de caminhão invalidaria o hodômetro de abertura já
// registrado; isso exigiria reabrir a rota, não uma simples edição.
public record UpdateRouteRequest(string RouteName, float Kilometers, int? TruckLoadId);

// RN-12: encerramento exige a leitura final do hodômetro.
public record CloseRouteRequest(float FinalOdometer, string? Note);

public record RouteResponse(
    int Id,
    string RouteName,
    float Kilometers,
    float? KilometersCovered,
    float StartOdometer,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool InProcess,
    int? TruckId,
    int? TruckLoadId,
    decimal TotalExpenses,
    float? UtilizationPercentage);
