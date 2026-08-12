/// Espelha `DriverProfileResponse` (backend/Dtos/AuthContracts.cs e
/// UserContracts.cs) — retornado por /login, GET /me e PUT /me/email.
class DriverProfile {
  const DriverProfile({
    required this.id,
    required this.name,
    required this.email,
  });

  final int id;
  final String name;
  final String email;

  factory DriverProfile.fromJson(Map<String, dynamic> json) {
    return DriverProfile(
      id: json['id'] as int,
      name: json['name'] as String,
      email: json['email'] as String,
    );
  }
}
