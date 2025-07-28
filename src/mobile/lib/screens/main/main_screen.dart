import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:go_router/go_router.dart';
import 'package:stajproje/utils/constants.dart';

class MainScreen extends StatelessWidget {
  const MainScreen({super.key});
  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          SizedBox(height: 75),
          Image.network(
            logoImageUrl,
            width: 200,
            errorBuilder:
                (context, error, stackTrace) =>
                    const Icon(Icons.credit_card, size: 80),
          ),
          const SizedBox(height: 24),
          const Text(
            'Dijital Kartvizitinizi Oluşturun',
            style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 12),
          const Text(
            'Kişisel ve profesyonel bilgilerinizi dijital ortamda güvenle saklayın, kolayca paylaşın.',
            style: TextStyle(fontSize: 16, color: Colors.grey),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 32),
          FButton(
            style: FButtonStyle.primary(),
            onPress: () {
              context.go('/auth');
            },
            child: const Text('Kartvizitini Oluştur'),
          ),
          const SizedBox(height: 16),
          FButton(
            style: FButtonStyle.outline(),
            onPress: () {
              context.go('/auth');
            },
            child: const Text('Profilini Görüntüle'),
          ),
          const SizedBox(height: 32),
          FCard(
            child: Column(
              spacing: 8,
              children: [
                FItem(
                  prefix: Icon(
                    FIcons.qrCode,
                    color: Colors.blueAccent,
                    size: 25,
                  ),
                  title: const Text('QR Kod ile Hızlı Paylaşım'),
                  onPress: () {},
                ),
                FItem(
                  prefix: Icon(
                    FIcons.lock,
                    color: Colors.greenAccent,
                    size: 25,
                  ),
                  title: const Text('Güvenli ve Kolay Saklama'),
                  onPress: () {},
                ),
                FItem(
                  prefix: Icon(
                    FIcons.pen,
                    color: Colors.orangeAccent,
                    size: 25,
                  ),
                  title: const Text('Kolayca Güncelleme'),
                  onPress: () {},
                ),
              ],
            ),
          ),
          const SizedBox(height: 32),
          TextButton(
            onPressed: () {
              context.push('/user-agreement');
            },
            child: const Text('Kullanıcı Sözleşmesi'),
          ),
        ],
      ),
    );
  }
}
