import 'package:flutter/material.dart';

import 'screens/auth_gate.dart';
import 'services/auth_service.dart';

void main() {
  runApp(DriverApp(authService: AuthService()));
}

class DriverApp extends StatelessWidget {
  const DriverApp({super.key, required this.authService});

  final AuthService authService;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Driver App',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
        useMaterial3: true,
      ),
      home: AuthGate(authService: authService),
    );
  }
}
