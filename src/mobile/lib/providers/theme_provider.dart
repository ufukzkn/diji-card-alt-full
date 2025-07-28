import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ThemeProvider with ChangeNotifier {
  ThemeMode _themeMode = ThemeMode.system;

  ThemeMode get themeMode => _themeMode;

  /// Temayı ayarlar ve SharedPreferences'a kaydeder.
  void setThemeMode(ThemeMode mode) async {
    _themeMode = mode;
    notifyListeners();

    final prefs = await SharedPreferences.getInstance();
    final themeString = mode.toString().split('.').last;
    await prefs.setString('themeMode', themeString);
  }

  /// Kayıtlı temayı yükler veya ilk kez uygulamayı açarken sistem temasına göre ayarlar.
  Future<void> loadThemeMode() async {
    final prefs = await SharedPreferences.getInstance();
    final themeString = prefs.getString('themeMode');

    if (themeString != null) {
      // Kayıtlı tema varsa, ona göre ayarla
      _themeMode = ThemeMode.values.firstWhere(
        (e) => e.toString().split('.').last == themeString,
        orElse: () => ThemeMode.system, // Bulunamazsa varsayılan olarak sistem
      );
    } else {
      // Kayıtlı tema yoksa, sistem temasına göre ayarla ve kaydet
      final brightness =
          WidgetsBinding.instance.platformDispatcher.platformBrightness;
      _themeMode =
          brightness == Brightness.dark ? ThemeMode.dark : ThemeMode.light;

      final initialThemeString = _themeMode.toString().split('.').last;
      await prefs.setString('themeMode', initialThemeString);
    }
    notifyListeners();
  }
}
