import 'driver_profile.dart';

/// Espelha `AuthResponse` (backend/Dtos/AuthContracts.cs), retornado por
/// POST /login.
class LoginResult {
  const LoginResult({
    required this.token,
    required this.expiresAtUtc,
    required this.driver,
  });

  final String token;
  final DateTime expiresAtUtc;
  final DriverProfile driver;

  factory LoginResult.fromJson(Map<String, dynamic> json) {
    return LoginResult(
      token: json['token'] as String,
      expiresAtUtc: DateTime.parse(json['expiresAtUtc'] as String),
      driver: DriverProfile.fromJson(json['driver'] as Map<String, dynamic>),
    );
  }
}
