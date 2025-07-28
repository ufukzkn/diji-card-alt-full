import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:stajproje/providers/theme_provider.dart';
import 'package:stajproje/providers/locale_provider.dart';
import 'package:stajproje/l10n/app_localizations.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  bool isPushEnabled = true;
  bool isTwoFactorEnabled = false;

  @override
  Widget build(BuildContext context) {
    //final themeProvider = Provider.of<ThemeProvider>(context);
    final themeProvider = context.watch<ThemeProvider>();
    // context.watch<ThemeProvider>().themeMode.toString() ile değişkenin değerini alırız
    // context.watch<ThemeProvider>().setThemeMode(ThemeMode.dark); ile değişkenin değerini değiştiririz

    final localeProvider = context.watch<LocaleProvider>();
    final l10n = AppLocalizations.of(context)!;

    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SizedBox(height: 30),

          // Profil Bilgileri Section
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Text(
              'Profil',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          FTileGroup(
            children: [
              FTile(
                prefix: Icon(FIcons.user),
                title: const Text('Profili Düzenle'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {
                  context.push('/main/profile');
                },
              ),
              FTile(
                prefix: Icon(FIcons.trash, color: Colors.redAccent),
                title: const Text(
                  'Hesabımı Sil',
                  style: TextStyle(color: Colors.redAccent),
                ),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {
                  // Hesap silme confirmation dialog'u
                },
              ),
              FTile(
                prefix: Icon(FIcons.logOut, color: Colors.redAccent),
                title: const Text(
                  'Çıkış Yap',
                  style: TextStyle(color: Colors.redAccent),
                ),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {
                  // Çıkış yapma logic'i
                  context.go('/');
                },
              ),
            ],
          ),

          const SizedBox(height: 24),

          // Güvenlik Section
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Text(
              'Güvenlik ve Gizlilik',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          FTileGroup(
            children: [
              FTile(
                prefix: Icon(FIcons.lock),
                title: const Text('Şifre Değiştir'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {},
              ),
              FTile(
                prefix: Icon(FIcons.shield),
                title: const Text('Gizlilik Ayarları'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {},
              ),
            ],
          ),

          const SizedBox(height: 24),

          // Uygulama Ayarları Section
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Text(
              'Uygulama Ayarları',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          FTileGroup(
            children: [
              FTile(
                prefix: Icon(FIcons.moon),
                title: const Text('Karanlık Mod'),
                suffix: FSwitch(
                  value: themeProvider.themeMode == ThemeMode.dark,
                  onChange: (value) {
                    themeProvider.setThemeMode(
                      value ? ThemeMode.dark : ThemeMode.light,
                    );
                  },
                ),
                onPress: () {},
              ),
              FTile(
                prefix: Icon(FIcons.globe),
                title: Text(l10n.language),
                subtitle: Text(
                  LocaleProvider.getLanguageNames(
                    localeProvider.locale,
                  )[localeProvider.locale.languageCode]!,
                ),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {
                  _showLanguageDialog(context, localeProvider, l10n);
                },
              ),
            ],
          ),

          const SizedBox(height: 24),

          // Destek Section
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Text(
              'Destek',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          FTileGroup(
            children: [
              FTile(
                prefix: Icon(FIcons.info),
                title: const Text('Yardım ve Destek'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {},
              ),
              FTile(
                prefix: Icon(FIcons.messageSquare),
                title: const Text('Geri Bildirim Gönder'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {},
              ),
              FTile(
                prefix: Icon(FIcons.star),
                title: const Text('Uygulamayı Değerlendir'),
                suffix: Icon(FIcons.chevronRight),
                onPress: () {},
              ),
            ],
          ),

          const SizedBox(height: 50),
        ],
      ),
    );
  }

  void _showLanguageDialog(
    BuildContext context,
    LocaleProvider localeProvider,
    AppLocalizations l10n,
  ) {
    // Seçilen dildeki dillerin adlarını alır
    final languageNames = LocaleProvider.getLanguageNames(
      localeProvider.locale,
    );
    final flagUrls = {
      'tr':
          'https://upload.wikimedia.org/wikipedia/commons/thumb/b/b4/Flag_of_Turkey.svg/1200px-Flag_of_Turkey.svg.png',
      'en':
          'https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Flag_of_the_United_Kingdom_%283-5%29.svg/1200px-Flag_of_the_United_Kingdom_%283-5%29.svg.png',
      'de':
          'https://upload.wikimedia.org/wikipedia/commons/thumb/b/ba/Flag_of_Germany.svg/2560px-Flag_of_Germany.svg.png',
    };

    showFDialog(
      context: context,
      builder:
          (context, style, animation) => FDialog.raw(
            builder:
                (context, style) => FTileGroup(
                  children:
                      LocaleProvider.supportedLocales.map((locale) {
                        final isSelected =
                            localeProvider.locale.languageCode ==
                            locale.languageCode;
                        return FTile(
                          prefix: Image.network(
                            flagUrls[locale.languageCode]!,
                            height: 15,
                            errorBuilder:
                                (context, error, stackTrace) =>
                                    Icon(FIcons.globe, size: 15),
                          ),
                          title: Text(languageNames[locale.languageCode]!),
                          suffix: isSelected ? Icon(FIcons.check) : null,
                          onPress: () {
                            localeProvider.setLocale(locale);
                            context.pop();
                          },
                        );
                      }).toList(),
                ),
          ),
    );
  }
}
