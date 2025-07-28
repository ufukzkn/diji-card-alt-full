import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:go_router/go_router.dart';
import 'package:stajproje/utils/alert.dart';

class LoginForm extends StatefulWidget {
  const LoginForm({super.key});

  @override
  State<LoginForm> createState() => _LoginFormState();
}

class _LoginFormState extends State<LoginForm> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FCard(
      title: const Text('Giriş yapın'),
      subtitle: const Text('Mevcut hesabınıza giriş yapın.'),
      child: Form(
        key: _formKey,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            FTextFormField(
              label: const Text('Kullanıcı adı'),
              controller: _usernameController,
              hint: 'johndoe',
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Kullanıcı adı boş bırakılamaz.';
                } else if (value.contains(' ')) {
                  return 'Kullanıcı adı boşluk içeremez.';
                }
                return null;
              },
            ),
            const SizedBox(height: 10),
            FTextFormField.password(
              label: const Text('Şifre'),
              controller: _passwordController,
              hint: 'Şifre',
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator:
                  (value) =>
                      8 <= (value?.length ?? 0)
                          ? null
                          : 'Şifre en az 8 karakterden oluşmalıdır.',
            ),
            const SizedBox(height: 20),
            FButton(
              child: const Text('Giriş Yap'),
              onPress: () {
                if (!_formKey.currentState!.validate()) {
                  return; // Form is invalid.
                }

                _formKey.currentState!.save();

                if (_usernameController.text == "admin" &&
                    _passwordController.text == "adminadmin") {
                  context.go('/main/home');
                  showCustomToast(
                    context: context,
                    title: "Bilgi",
                    message: "Başarıyla giriş yaptınız.",
                    type: AlertType.success,
                  );
                } else {
                  showCustomToast(
                    context: context,
                    title: "Bilgi",
                    message: "Kullanıcı adı veya şifre hatalı.",
                    type: AlertType.error,
                  );
                }
              },
            ),
            const SizedBox(height: 10),
            FButton(
              style: FButtonStyle.ghost(),
              onPress: () {
                context.push('/forgot-password');
              },
              child: const Text('Şifremi unuttum'),
            ),
          ],
        ),
      ),
    );
  }
}
