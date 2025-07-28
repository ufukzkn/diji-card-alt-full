import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class LocaleProvider with ChangeNotifier {
  // Varsayılan dil Türkçe
  Locale _locale = const Locale('tr');

  Locale get locale => _locale;

  void setLocale(Locale locale) async {
    _locale = locale;
    notifyListeners();

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('languageCode', locale.languageCode);
  }

  Future<void> loadLocale() async {
    final prefs = await SharedPreferences.getInstance();
    final languageCode = prefs.getString('languageCode');

    if (languageCode != null) {
      _locale = Locale(languageCode);
    }
    notifyListeners();
  }

  static const List<Locale> supportedLocales = [
    Locale('tr'),
    Locale('en'),
    Locale('de'),
  ];

  static Map<String, String> getLanguageNames(Locale currentLocale) {
    switch (currentLocale.languageCode) {
      case 'tr':
        return {'tr': 'Türkçe', 'en': 'İngilizce', 'de': 'Almanca'};
      case 'en':
        return {'tr': 'Turkish', 'en': 'English', 'de': 'German'};
      case 'de':
        return {'tr': 'Türkisch', 'en': 'Englisch', 'de': 'Deutsch'};
      default:
        return {'tr': 'Türkçe', 'en': 'İngilizce', 'de': 'Almanca'};
    }
  }
}
