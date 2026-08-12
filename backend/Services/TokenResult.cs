namespace backend.Services;

public record TokenResult(string Token, DateTime ExpiresAtUtc);
