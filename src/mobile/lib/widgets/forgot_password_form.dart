import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:stajproje/widgets/fade_in_wrapper.dart';
import 'package:forui/forui.dart';
//import 'package:stajproje/widgets/otp_input.dart';

class ForgotPasswordForm extends StatefulWidget {
  const ForgotPasswordForm({super.key});

  @override
  State<ForgotPasswordForm> createState() => _ForgotPasswordFormState();
}

class _ForgotPasswordFormState extends State<ForgotPasswordForm> {
  int _step = 0;

  void _goToNextStep() {
    setState(() {
      _step++;
    });
  }

  @override
  Widget build(BuildContext context) {
    return switch (_step) {
      0 => FadeInWrapper(
        child: _ForgotPasswordStep0(onNextStep: _goToNextStep),
      ),
      1 => FadeInWrapper(
        child: _ForgotPasswordStep1(onNextStep: _goToNextStep),
      ),
      2 => FadeInWrapper(child: _ForgotPasswordStep2()),
      _ => const SizedBox.shrink(),
    };
  }
}

// 0. step: E-posta adresi girmek için
class _ForgotPasswordStep0 extends StatefulWidget {
  final VoidCallback onNextStep;

  const _ForgotPasswordStep0({super.key, required this.onNextStep});

  @override
  State<_ForgotPasswordStep0> createState() => __ForgotPasswordStep0State();
}

class __ForgotPasswordStep0State extends State<_ForgotPasswordStep0> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _emailController = TextEditingController();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FCard(
      title: const Text('Şifrenizi sıfırlayın'),
      subtitle: const Text(
        'Hesabınızın şifresini sıfırlamak için lütfen e-posta adresinizi giriniz.',
      ),
      child: Form(
        key: _formKey,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
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
            const SizedBox(height: 20),
            FButton(
              prefix: _isLoading ? const FProgress.circularIcon() : null,
              onPress:
                  _isLoading
                      ? null
                      : () async {
                        if (!_formKey.currentState!.validate()) {
                          return; // Form is invalid.
                        }

                        _formKey.currentState!.save();

                        setState(() {
                          _isLoading = true;
                        });

                        // API isteği için simüle
                        await Future.delayed(const Duration(seconds: 2));

                        // mounted keyword'u, widget'ın hala aktif olup olmadığını kontrol eder.
                        if (mounted) {
                          setState(() {
                            _isLoading = false;
                          });

                          widget.onNextStep();
                        }
                      },
              child: Text(_isLoading ? 'Lütfen bekleyin...' : 'Devam Et'),
            ),
          ],
        ),
      ),
    );
  }
}

// 1. step: Yeni şifreyi girmek için
class _ForgotPasswordStep1 extends StatefulWidget {
  final VoidCallback onNextStep;

  const _ForgotPasswordStep1({super.key, required this.onNextStep});

  @override
  State<_ForgotPasswordStep1> createState() => __ForgotPasswordStep1State();
}

class __ForgotPasswordStep1State extends State<_ForgotPasswordStep1> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _codeController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _passwordAgainController =
      TextEditingController();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _codeController.dispose();
    _passwordController.dispose();
    _passwordAgainController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FCard(
      title: const Text('Şifrenizi sıfırlayın'),
      subtitle: const Text(
        'Hesabınızın şifresini sıfırlamak için lütfen e-posta adresinize gönderilen kodu ve yeni şifrenizi giriniz.',
      ),
      child: Form(
        key: _formKey,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            /*OtpInput(
              boxCount: 6,
              onChanged: (value) {
                print(value);
              },
            ),*/
            FTextFormField(
              label: const Text('Kod'),
              controller: _codeController,
              hint: 'xxxxxx',
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Kod alanı boş bırakılamaz.';
                } else if (value.contains(' ')) {
                  return 'Kod alanı boşluk içeremez.';
                }
                return null;
              },
            ),
            const SizedBox(height: 10),
            FTextFormField.password(
              label: const Text('Yeni şifre'),
              controller: _passwordController,
              hint: 'Yeni şifre',
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
            const SizedBox(height: 20),
            FButton(
              prefix: _isLoading ? const FProgress.circularIcon() : null,
              onPress:
                  _isLoading
                      ? null
                      : () async {
                        if (!_formKey.currentState!.validate()) {
                          return; // Form is invalid.
                        }

                        _formKey.currentState!.save();

                        setState(() {
                          _isLoading = true;
                        });

                        // API isteği için simüle
                        await Future.delayed(const Duration(seconds: 2));

                        // mounted keyword'u, widget'ın hala aktif olup olmadığını kontrol eder.
                        if (mounted) {
                          setState(() {
                            _isLoading = false;
                          });

                          widget.onNextStep();
                        }
                      },
              child: Text(_isLoading ? 'Lütfen bekleyin...' : 'Devam Et'),
            ),
          ],
        ),
      ),
    );
  }
}

// 2. step: Şifre sıfırlama işlemi tamamlandı
class _ForgotPasswordStep2 extends StatelessWidget {
  const _ForgotPasswordStep2({super.key});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        children: [
          const Icon(Icons.check_circle, color: Colors.green, size: 50),
          SizedBox(height: 10),
          const Text(
            'Şifreniz başarıyla sıfırlandı.',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 20),
          FButton(
            child: const Text('Giriş yap'),
            onPress: () {
              context.pushReplacement('/auth', extra: 'login');
            },
          ),
        ],
      ),
    );
  }
}
