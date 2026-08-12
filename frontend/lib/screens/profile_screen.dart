import 'package:flutter/material.dart';

import '../core/api_exception.dart';
import '../models/driver_profile.dart';
import '../services/auth_service.dart';
import 'login_screen.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({
    super.key,
    required this.authService,
    required this.initialProfile,
  });

  final AuthService authService;
  final DriverProfile initialProfile;

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  late DriverProfile _profile;

  @override
  void initState() {
    super.initState();
    _profile = widget.initialProfile;
  }

  Future<void> _refreshProfile() async {
    try {
      final profile = await widget.authService.currentProfile();
      if (!mounted) return;
      if (profile == null) {
        _goToLogin();
        return;
      }
      setState(() => _profile = profile);
    } on ApiException {
      // Falha transitória (rede fora do ar, 5xx) ao atualizar: mantém os
      // dados já exibidos em vez de travar a tela com um erro bloqueante.
    }
  }

  void _goToLogin() {
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => LoginScreen(authService: widget.authService)),
      (route) => false,
    );
  }

  Future<void> _logout() async {
    await widget.authService.logout();
    _goToLogin();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Meu perfil'),
        actions: [
          IconButton(
            onPressed: _logout,
            icon: const Icon(Icons.logout),
            tooltip: 'Sair',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _refreshProfile,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Nome', style: Theme.of(context).textTheme.labelMedium),
                    Text(_profile.name, style: Theme.of(context).textTheme.titleMedium),
                    const SizedBox(height: 12),
                    Text('E-mail', style: Theme.of(context).textTheme.labelMedium),
                    Text(_profile.email, style: Theme.of(context).textTheme.titleMedium),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),
            _UpdateEmailSection(
              authService: widget.authService,
              onUpdated: (updated) => setState(() => _profile = updated),
              onSessionExpired: _goToLogin,
            ),
            const SizedBox(height: 24),
            _UpdatePasswordSection(
              authService: widget.authService,
              onSessionExpired: _goToLogin,
            ),
          ],
        ),
      ),
    );
  }
}

class _UpdateEmailSection extends StatefulWidget {
  const _UpdateEmailSection({
    required this.authService,
    required this.onUpdated,
    required this.onSessionExpired,
  });

  final AuthService authService;
  final ValueChanged<DriverProfile> onUpdated;
  final VoidCallback onSessionExpired;

  @override
  State<_UpdateEmailSection> createState() => _UpdateEmailSectionState();
}

class _UpdateEmailSectionState extends State<_UpdateEmailSection> {
  final _formKey = GlobalKey<FormState>();
  final _passwordController = TextEditingController();
  final _newEmailController = TextEditingController();

  bool _isSubmitting = false;
  String? _errorMessage;

  @override
  void dispose() {
    _passwordController.dispose();
    _newEmailController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
    });

    try {
      final updated = await widget.authService.updateEmail(
        password: _passwordController.text,
        newEmail: _newEmailController.text.trim(),
      );
      if (!mounted) return;
      widget.onUpdated(updated);
      _passwordController.clear();
      _newEmailController.clear();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('E-mail atualizado com sucesso.')),
      );
    } on SessionExpiredException {
      widget.onSessionExpired();
    } on ApiException catch (e) {
      setState(() => _errorMessage = e.message);
    } catch (_) {
      setState(() {
        _errorMessage = 'Não foi possível conectar ao servidor. Tente novamente.';
      });
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Alterar e-mail', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 16),
              TextFormField(
                controller: _passwordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Senha atual',
                  border: OutlineInputBorder(),
                ),
                validator: (value) =>
                    (value == null || value.isEmpty) ? 'Informe sua senha atual' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _newEmailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Novo e-mail',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  final v = value?.trim() ?? '';
                  if (v.isEmpty || !v.contains('@')) {
                    return 'Informe um e-mail válido';
                  }
                  return null;
                },
              ),
              if (_errorMessage != null) ...[
                const SizedBox(height: 12),
                Text(
                  _errorMessage!,
                  style: TextStyle(color: Theme.of(context).colorScheme.error),
                ),
              ],
              const SizedBox(height: 16),
              Align(
                alignment: Alignment.centerRight,
                child: FilledButton(
                  onPressed: _isSubmitting ? null : _submit,
                  child: _isSubmitting
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Salvar e-mail'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _UpdatePasswordSection extends StatefulWidget {
  const _UpdatePasswordSection({
    required this.authService,
    required this.onSessionExpired,
  });

  final AuthService authService;
  final VoidCallback onSessionExpired;

  @override
  State<_UpdatePasswordSection> createState() => _UpdatePasswordSectionState();
}

class _UpdatePasswordSectionState extends State<_UpdatePasswordSection> {
  final _formKey = GlobalKey<FormState>();
  final _currentPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();

  bool _isSubmitting = false;
  String? _errorMessage;

  @override
  void dispose() {
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
    });

    try {
      await widget.authService.updatePassword(
        currentPassword: _currentPasswordController.text,
        newPassword: _newPasswordController.text,
      );
      if (!mounted) return;
      _currentPasswordController.clear();
      _newPasswordController.clear();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Senha atualizada com sucesso.')),
      );
    } on SessionExpiredException {
      widget.onSessionExpired();
    } on ApiException catch (e) {
      setState(() => _errorMessage = e.message);
    } catch (_) {
      setState(() {
        _errorMessage = 'Não foi possível conectar ao servidor. Tente novamente.';
      });
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Alterar senha', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 16),
              TextFormField(
                controller: _currentPasswordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Senha atual',
                  border: OutlineInputBorder(),
                ),
                validator: (value) =>
                    (value == null || value.isEmpty) ? 'Informe sua senha atual' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _newPasswordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Nova senha',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.length < 6) {
                    return 'A nova senha precisa ter ao menos 6 caracteres';
                  }
                  return null;
                },
              ),
              if (_errorMessage != null) ...[
                const SizedBox(height: 12),
                Text(
                  _errorMessage!,
                  style: TextStyle(color: Theme.of(context).colorScheme.error),
                ),
              ],
              const SizedBox(height: 16),
              Align(
                alignment: Alignment.centerRight,
                child: FilledButton(
                  onPressed: _isSubmitting ? null : _submit,
                  child: _isSubmitting
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Salvar senha'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
