import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Guarda o token JWT em armazenamento criptografado do SO (Keystore no
/// Android, Keychain no iOS/macOS) — nunca `SharedPreferences` puro aqui,
/// já que ele grava em texto plano (exigência da task).
class TokenStorage {
  TokenStorage({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const _tokenKey = 'driver_app_auth_token';

  Future<void> saveToken(String token) => _storage.write(key: _tokenKey, value: token);

  Future<String?> readToken() => _storage.read(key: _tokenKey);

  Future<void> deleteToken() => _storage.delete(key: _tokenKey);
}
