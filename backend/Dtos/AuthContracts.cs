namespace backend.Dtos;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record DriverProfileResponse(int Id, string Name, string Email);

public record AuthResponse(string Token, DateTime ExpiresAtUtc, DriverProfileResponse Driver);
