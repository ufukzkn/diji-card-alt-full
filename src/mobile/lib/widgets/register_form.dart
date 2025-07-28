import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';

class RegisterForm extends StatefulWidget {
  const RegisterForm({super.key});

  @override
  State<RegisterForm> createState() => _RegisterFormState();
}

class _RegisterFormState extends State<RegisterForm> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _passwordAgainController =
      TextEditingController();

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _passwordAgainController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FCard(
      title: const Text('Kayıt olun'),
      subtitle: const Text(
        'Dijital dünyada kendinizi tanıtmak için kayıt olun.',
      ),
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
            FTextFormField.email(
              label: const Text('E-posta'),
              controller: _emailController,
              hint: 'john@doe.com',
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator:
                  (value) =>
                      (value?.contains('@') ?? false)
                          ? null
                          : 'Lütfen geçerli bir e-posta adresi giriniz.',
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
            const SizedBox(height: 10),
            FTextFormField.password(
              label: const Text('Şifre (tekrar)'),
              controller: _passwordAgainController,
              hint: 'Şifre (tekrar)',
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Şifre tekrarı boş bırakılamaz.';
                } else if (value.length < 8) {
                  return 'Şifre en az 8 karakterden oluşmalıdır.';
                } else if (value != _passwordController.text) {
                  return 'Şifreler eşleşmiyor.';
                }
                return null;
              },
            ),
            const SizedBox(height: 10),
            FormField(
              initialValue: false,
              onSaved: (value) {
                // Save values somewhere.
              },
              validator:
                  (value) =>
                      (value ?? false)
                          ? null
                          : 'Lütfen sözleşmeleri kabul edin.',
              builder:
                  (state) => FCheckbox(
                    label: const Text('Kullanıcı sözleşmesini kabul ediyorum.'),
                    description: GestureDetector(
                      child: const Text('Sözleşme için buraya tıklayın.'),
                      onTap: () {
                        context.push('/user-agreement');
                      },
                    ),
                    error:
                        state.errorText != null ? Text(state.errorText!) : null,
                    value: state.value ?? false,
                    onChange: (value) => state.didChange(value),
                  ),
            ),
            const SizedBox(height: 20),
            FButton(
              child: const Text('Kayıt Ol'),
              onPress: () {
                if (!_formKey.currentState!.validate()) {
                  return; // Form is invalid.
                }

                _formKey.currentState!.save();

                // Form is valid, do something.
              },
            ),
          ],
        ),
      ),
    );
  }
}
