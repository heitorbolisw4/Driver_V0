import 'dart:convert';

import 'package:http/http.dart' as http;

/// Client HTTP fino sobre a API do Driver App. Não conhece regra de negócio
/// nem trata status code — isso é responsabilidade de quem chama
/// (ver `services/auth_service.dart`).
class ApiClient {
  ApiClient({http.Client? httpClient, String? baseUrl})
      : _httpClient = httpClient ?? http.Client(),
        _baseUrl = baseUrl ?? _defaultBaseUrl;

  final http.Client _httpClient;
  final String _baseUrl;

  // Base URL configurável em build/run, sem precisar recompilar o app pra
  // trocar de ambiente:
  //   flutter run --dart-define=API_BASE_URL=http://192.168.0.10:5248
  //
  // Default aponta pro perfil "http" do backend (ver
  // backend/Properties/launchSettings.json) através do 10.0.2.2, o alias que
  // o emulador Android usa pro localhost da máquina host — "localhost" direto
  // não funciona de dentro do emulador.
  //
  // Cleartext HTTP só é liberado no build de debug para esse host específico
  // (ver android/app/src/debug/res/xml/network_security_config.xml). Rodando
  // em dispositivo físico, troque o dart-define pelo IP da máquina na rede
  // local e adicione esse IP no mesmo arquivo de network security config.
  static const _defaultBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5248',
  );

  Uri _uri(String path) => Uri.parse('$_baseUrl$path');

  Map<String, String> _headers({String? token}) => {
        'Content-Type': 'application/json; charset=utf-8',
        if (token != null) 'Authorization': 'Bearer $token',
      };

  Future<http.Response> get(String path, {String? token}) {
    return _httpClient.get(_uri(path), headers: _headers(token: token));
  }

  Future<http.Response> post(
    String path, {
    Map<String, dynamic>? body,
    String? token,
  }) {
    return _httpClient.post(
      _uri(path),
      headers: _headers(token: token),
      body: body == null ? null : jsonEncode(body),
    );
  }

  Future<http.Response> put(
    String path, {
    Map<String, dynamic>? body,
    String? token,
  }) {
    return _httpClient.put(
      _uri(path),
      headers: _headers(token: token),
      body: body == null ? null : jsonEncode(body),
    );
  }
}
