// Teste de fumaça: garante que a tela de Login sobe sem estourar e expõe os
// campos esperados. Não cobre o fluxo de autenticação em si — isso depende
// do backend rodando (fora do escopo de um widget test).

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:frontend/screens/login_screen.dart';
import 'package:frontend/services/auth_service.dart';

void main() {
  testWidgets('LoginScreen renderiza campos de e-mail e senha', (tester) async {
    await tester.pumpWidget(
      MaterialApp(home: LoginScreen(authService: AuthService())),
    );

    expect(find.text('Entrar'), findsWidgets);
    expect(find.byType(TextFormField), findsNWidgets(2));
  });
}
