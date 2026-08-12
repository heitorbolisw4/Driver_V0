namespace backend.Dtos;

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword);

public record UpdateEmailRequest(string Password, string NewEmail);
