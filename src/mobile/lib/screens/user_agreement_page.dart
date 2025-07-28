import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:go_router/go_router.dart';

class UserAgreementPage extends StatelessWidget {
  const UserAgreementPage({super.key});

  @override
  Widget build(BuildContext context) {
    return FScaffold(
      header: FHeader.nested(
        title: const Text('Kullanıcı Sözleşmesi'),
        prefixes: [
          FHeaderAction.back(
            onPress: () {
              context.pop();
            },
          ),
        ],
      ),
      child: Center(
        child: SingleChildScrollView(
          child: RichText(
            text: TextSpan(
              style: TextStyle(
                fontSize: 16,
                height: 1.5,
                fontFamily: 'Montserrat',
                color: context.theme.colors.primary,
              ),
              children: const [
                TextSpan(
                  text: '1. Giriş\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      'Bu kullanıcı sözleşmesi ("Sözleşme"), OctaDigital ("OctaDigital") ile siteyi kullanan kullanıcılar ("Kullanıcı" veya "Siz") arasında yapılmıştır. Siteyi kullanarak bu Sözleşme şartlarını kabul etmiş sayılırsınız.\n\n',
                ),
                TextSpan(
                  text: '2. Hizmetin Kullanımı\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Site, Kullanıcılara [site hizmetlerinin kısa tanımı] sunar.\n- Hizmeti kullanırken yürürlükteki kanunlara, ahlaka ve Site kurallarına uygun davranmayı kabul edersiniz.\n\n',
                ),
                TextSpan(
                  text: '3. Kayıt ve Hesap Güvenliği\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Siteye kayıt olurken verdiğiniz bilgilerin doğru, güncel ve eksiksiz olması gerekmektedir.\n- Hesap bilgilerinizin gizliliğini sağlamak ve hesabınızın yetkisiz kullanımlarına karşı korunmasından siz sorumlusunuz.\n- Şüpheli durumlarda Site yönetimini derhal bilgilendirmelisiniz.\n\n',
                ),
                TextSpan(
                  text: '4. Kullanıcı İçeriği\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Siteye yüklediğiniz içeriklerin (metin, fotoğraf, video vb.) hukuka aykırı, başkalarının haklarını ihlal edici veya zararlı olmaması gerekmektedir.\n- Kullanıcı içeriği nedeniyle oluşabilecek her türlü yasal sorumluluk size aittir.\n- Site, kullanıcı içeriklerini önceden inceleme ve hukuka aykırı bulduğu içerikleri kaldırma hakkını saklı tutar.\n\n',
                ),
                TextSpan(
                  text: '5. Fikri Mülkiyet Hakları\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Siteye ait her türlü içerik, marka, tasarım ve yazılım Telif Hakları ve diğer fikri mülkiyet hakları ile korunmaktadır.\n- Site içeriğini, yazılı izin olmadan çoğaltmak, dağıtmak veya üçüncü şahıslara aktarmak yasaktır.\n\n',
                ),
                TextSpan(
                  text: '6. Sorumluluk Reddi\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Site, hizmetlerin kesintisiz, hatasız ve güvenli olacağına dair garanti vermez.\n- Siteyi kullanmanızdan doğabilecek zararlardan Site yönetimi sorumlu tutulamaz.\n\n',
                ),
                TextSpan(
                  text: '7. Gizlilik ve Kişisel Veriler\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Site, kişisel verilerinizi [Gizlilik Politikası] kapsamında korur.\n- Kullanıcı olarak kişisel verilerinizin işlenmesini kabul etmiş sayılırsınız.\n\n',
                ),
                TextSpan(
                  text: '8. Sözleşmenin Değiştirilmesi\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Site, bu sözleşmede değişiklik yapma hakkını saklı tutar.\n- Değişiklikler yayım tarihinden itibaren geçerli olur ve site üzerinden duyurulur.\n\n',
                ),
                TextSpan(
                  text: '9. Sözleşmenin Feshi\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Kurallara aykırı davranışlarda bulunan kullanıcıların hesapları uyarı veya kalıcı olarak kapatılabilir.\n\n',
                ),
                TextSpan(
                  text: '10. Uyuşmazlıkların Çözümü\n\n',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                TextSpan(
                  text:
                      '- Bu sözleşmeden doğan uyuşmazlıklarda Türkiye mahkemeleri yetkilidir.\n\n',
                ),
                TextSpan(text: '---\n\n'),
                TextSpan(
                  text: 'OctaDigital Yönetimi',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
