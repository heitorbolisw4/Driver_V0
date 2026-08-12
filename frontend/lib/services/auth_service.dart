import 'dart:convert';

import '../core/api_client.dart';
import '../core/api_exception.dart';
import '../core/token_storage.dart';
import '../models/driver_profile.dart';
import '../models/login_result.dart';

/// Regra de negócio de autenticação/perfil — única peça que conhece tanto a
/// API quanto o storage do token. Telas não falam com `ApiClient` diretamente.
class AuthService {
  AuthService({ApiClient? apiClient, TokenStorage? tokenStorage})
      : _apiClient = apiClient ?? ApiClient(),
        _tokenStorage = tokenStorage ?? TokenStorage();

  final ApiClient _apiClient;
  final TokenStorage _tokenStorage;

  /// POST /login. Lança [ApiException] em qualquer falha (401 já vem com
  /// mensagem amigável fixa, os demais casos usam a mensagem do backend).
  Future<DriverProfile> login(String email, String password) async {
    final response = await _apiClient.post('/login', body: {
      'email': email,
      'password': password,
    });

    if (response.statusCode == 401) {
      throw const ApiException(statusCode: 401, message: 'E-mail ou senha inválidos');
    }
    if (response.statusCode != 200) {
      throw ApiException.fromResponse(response);
    }

    final result = LoginResult.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    await _tokenStorage.saveToken(result.token);
    return result.driver;
  }

  /// GET /me usando o token salvo. Retorna `null` se não houver token salvo
  /// ou se o backend recusar o token (401) — nesse segundo caso o token
  /// salvo é descartado, então uma chamada logo em seguida também dá null.
  ///
  /// Usado tanto pra validar a sessão ao abrir o app quanto pra atualizar a
  /// tela de Perfil.
  Future<DriverProfile?> currentProfile() async {
    final token = await _tokenStorage.readToken();
    if (token == null) return null;

    final response = await _apiClient.get('/me', token: token);
    if (response.statusCode == 401) {
      await _tokenStorage.deleteToken();
      return null;
    }
    if (response.statusCode != 200) {
      throw ApiException.fromResponse(response);
    }

    return DriverProfile.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// PUT /me/email. Lança [SessionExpiredException] se o token não existir
  /// mais ou tiver expirado — a UI deve redirecionar pro Login nesse caso.
  Future<DriverProfile> updateEmail({
    required String password,
    required String newEmail,
  }) async {
    final token = await _requireToken();
    final response = await _apiClient.put('/me/email', token: token, body: {
      'password': password,
      'newEmail': newEmail,
    });

    if (response.statusCode == 401) {
      await _tokenStorage.deleteToken();
      throw const SessionExpiredException();
    }
    if (response.statusCode != 200) {
      throw ApiException.fromResponse(response);
    }

    return DriverProfile.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// PUT /me/password. Sucesso é 204 sem corpo.
  Future<void> updatePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final token = await _requireToken();
    final response = await _apiClient.put('/me/password', token: token, body: {
      'currentPassword': currentPassword,
      'newPassword': newPassword,
    });

    if (response.statusCode == 401) {
      await _tokenStorage.deleteToken();
      throw const SessionExpiredException();
    }
    if (response.statusCode != 204) {
      throw ApiException.fromResponse(response);
    }
  }

  Future<void> logout() => _tokenStorage.deleteToken();

  Future<String> _requireToken() async {
    final token = await _tokenStorage.readToken();
    if (token == null) {
      throw const SessionExpiredException();
    }
    return token;
  }
}
