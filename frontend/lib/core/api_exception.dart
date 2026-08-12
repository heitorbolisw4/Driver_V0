import 'dart:convert';

import 'package:http/http.dart' as http;

/// Erro retornado pela API com uma mensagem já pronta pra exibir na UI.
///
/// Tenta extrair `{"message": "..."}` do corpo da resposta (formato usado por
/// todos os endpoints do backend, ver `backend/Endpoints`); se o corpo não
/// vier nesse formato, cai pra uma mensagem genérica.
class ApiException implements Exception {
  const ApiException({required this.statusCode, required this.message});

  final int statusCode;
  final String message;

  factory ApiException.fromResponse(
    http.Response response, {
    String fallback = 'Não foi possível completar a operação. Tente novamente.',
  }) {
    var message = fallback;
    if (response.body.isNotEmpty) {
      try {
        final decoded = jsonDecode(response.body);
        if (decoded is Map<String, dynamic> && decoded['message'] is String) {
          message = decoded['message'] as String;
        }
      } catch (_) {
        // Corpo não é um JSON válido (ex: erro 500 sem payload estruturado);
        // mantém a mensagem genérica em vez de propagar o erro de parsing.
      }
    }
    return ApiException(statusCode: response.statusCode, message: message);
  }

  @override
  String toString() => message;
}

/// Sinaliza que o token salvo não existe mais ou o backend recusou (401) numa
/// chamada autenticada. A UI deve limpar o estado local e voltar pro Login —
/// não existe endpoint de refresh no backend, então não há o que tentar além
/// disso (ver RN sobre reautenticação no doc de regras de negócio).
class SessionExpiredException implements Exception {
  const SessionExpiredException();
}
