import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';
import 'package:stajproje/widgets/forgot_password_form.dart';

class ForgotPasswordPage extends StatelessWidget {
  const ForgotPasswordPage({super.key});

  @override
  Widget build(BuildContext context) {
    return FScaffold(
      header: FHeader.nested(
        title: const Text('Şifremi Unuttum'),
        prefixes: [
          FHeaderAction.back(
            onPress: () {
              context.go('/auth');
            },
          ),
        ],
      ),
      child: ForgotPasswordForm(),
    );
  }
}
