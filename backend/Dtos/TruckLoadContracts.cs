namespace backend.Dtos;

public record CreateTruckLoadRequest(string Type, float Weight, bool IsShipped);

public record UpdateTruckLoadRequest(string Type, float Weight, bool IsShipped);

public record TruckLoadResponse(int Id, string Type, float Weight, bool IsShipped);
