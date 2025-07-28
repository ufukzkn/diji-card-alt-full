import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';
import 'package:stajproje/widgets/login_form.dart';
import 'package:stajproje/widgets/register_form.dart';
import 'package:stajproje/widgets/fade_in_wrapper.dart';

class AuthScreen extends StatefulWidget {
  const AuthScreen({super.key});

  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final extra = GoRouterState.of(context).extra;
    return FScaffold(
      header: FHeader.nested(
        title: const Text('Üyelik'),
        prefixes: [
          FHeaderAction.back(
            onPress: () {
              context.go('/');
            },
          ),
        ],
      ),
      // Klavye açıldığında taşmayı önlemek için SingleChildScrollView
      child: SingleChildScrollView(
        child: FTabs(
          initialIndex: extra == 'login' ? 0 : 1,
          children: [
            FTabEntry(
              label: const Text('Giriş Yap'),
              child: FadeInWrapper(child: LoginForm()),
            ),
            FTabEntry(
              label: const Text('Kayıt Ol'),
              child: FadeInWrapper(child: RegisterForm()),
            ),
          ],
        ),
      ),
    );
  }
}
