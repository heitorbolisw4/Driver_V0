import 'package:flutter/material.dart';

import '../models/driver_profile.dart';
import '../services/auth_service.dart';
import 'login_screen.dart';
import 'profile_screen.dart';

/// Decide a tela inicial: valida o token salvo contra GET /me e manda pro
/// Perfil se ainda for válido, ou pro Login caso contrário (sem token, token
/// expirado, ou falha ao validar — nesse último caso não trava o usuário
/// numa tela de erro, só assume "sem sessão" e deixa logar de novo).
class AuthGate extends StatefulWidget {
  const AuthGate({super.key, required this.authService});

  final AuthService authService;

  @override
  State<AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<AuthGate> {
  late final Future<DriverProfile?> _initialProfile;

  @override
  void initState() {
    super.initState();
    _initialProfile = widget.authService.currentProfile();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<DriverProfile?>(
      future: _initialProfile,
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        final profile = snapshot.hasError ? null : snapshot.data;
        if (profile != null) {
          return ProfileScreen(
            authService: widget.authService,
            initialProfile: profile,
          );
        }
        return LoginScreen(authService: widget.authService);
      },
    );
  }
}
