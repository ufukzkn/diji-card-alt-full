import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';
import '../utils/constants.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return FScaffold(
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          spacing: 10,
          children: [
            Image.network(
              width: 250,
              logoImageUrl,
              loadingBuilder: (context, child, loadingProgress) {
                if (loadingProgress == null) return child;
                return CircularProgressIndicator();
              },
              errorBuilder: (context, error, stackTrace) {
                return Text('Resim yüklenemedi');
              },
            ),
            const Text(
              'OctaDigital',
              textAlign: TextAlign.center,
              style: TextStyle(fontWeight: FontWeight.w600, fontSize: 40),
            ),
            const Text(
              'Kartvizitin Dijital Hâli',
              textAlign: TextAlign.center,
              style: TextStyle(fontWeight: FontWeight.w600, fontSize: 25),
            ),
            const Text(
              'Hemen başla, dijital kartvizitini oluştur.',
              textAlign: TextAlign.center,
              style: TextStyle(fontWeight: FontWeight.w500, fontSize: 15),
            ),
            FButton(
              style: FButtonStyle.outline(),
              prefix: Icon(FIcons.logIn),
              onPress: () {
                context.push('/auth', extra: 'login');
              },
              child: const Text('Giriş Yap'),
            ),
            FButton(
              prefix: Icon(FIcons.plus),
              onPress: () {
                context.push('/auth', extra: 'register');
              },
              child: const Text('Kayıt Ol'),
            ),
          ],
        ),
      ),
    );
  }
}
