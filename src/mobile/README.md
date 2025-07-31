# 📱 Dijital Kartvizit Uygulaması

<div align="center">
  <img src="https://meet.octapull.com/assets/images/octapull-dark-icon.png" alt="App Logo" width="120" height="120">
  
  **Modern ve kullanıcı dostu dijital kartvizit uygulaması**
  
  [![Flutter](https://img.shields.io/badge/Flutter-02569B?style=for-the-badge&logo=flutter&logoColor=white)](https://flutter.dev/)
  [![Dart](https://img.shields.io/badge/Dart-0175C2?style=for-the-badge&logo=dart&logoColor=white)](https://dart.dev/)
</div>

## 🚀 Proje Hakkında

Bu uygulama, kullanıcıların kişisel ve profesyonel bilgilerini dijital ortamda güvenle saklayarak, kolayca paylaşabilmelerini sağlayan modern bir dijital kartvizit platformudur. QR kod teknolojisi ile hızlı paylaşım imkanı sunar.

## ✨ Özellikler

### 🎨 Kullanıcı Arayüzü
- **Modern UI/UX Tasarımı**: ForUI framework ile geliştirilmiş
- **Koyu/Açık Tema Desteği**: Kullanıcı tercihine göre tema değiştirme
- **Responsive Design**: Her ekran boyutuna uyumlu tasarım
- **Montserrat Font**: Profesyonel tipografi

### 🌍 Çoklu Dil Desteği
- 🇹🇷 **Türkçe** (Ana dil)
- 🇺🇸 **İngilizce**
- 🇩🇪 **Almanca**

### 👤 Kullanıcı Yönetimi
- Güvenli giriş/kayıt sistemi (Back-end bağlantısı eklenmedi.)
- Profil düzenleme ve yönetimi (Back-end bağlantısı eklenmedi.)
- Şifre sıfırlama özelliği (Back-end bağlantısı eklenmedi.)
- Hesap güvenlik ayarları (Back-end bağlantısı eklenmedi.)

### 💼 Kartvizit Özellikları
- **Özelleştirilebilir Tasarımlar**: Farklı kartvizit şablonları
- **Çoklu Kartvizit Desteği**: Birden fazla kartvizit oluşturma
- **QR Kod Entegrasyonu**: Kartvizitlere otomatik QR kod ekleme
- **Anlık Paylaşım**: QR kod ile hızlı bilgi paylaşımı

### 📱 Teknik Özellikler
- **QR Kod Tarayıcısı**: Kamera ile QR kod okuma
- **Kamera İzinleri**: Güvenli kamera erişimi
- **Veri Saklama**: Güvenli yerel veri depolama

## 🛠️ Teknoloji Stack'i

### Framework & Language
- **Flutter** ^3.7.2 - Cross-platform mobile development
- **Dart** - Programlama dili

### Ana Kütüphaneler
- **ForUI** ^0.13.1 - Modern UI component kütüphanesi
- **GoRouter** ^16.0.0 - Declarative routing
- **Provider** ^6.1.5 - State management
- **flutter_localizations** - Çoklu dil desteği

### Özellik Kütüphaneleri
- **mobile_scanner** ^7.0.1 - QR kod tarama
- **image_picker** ^1.1.2 - Resim seçme
- **permission_handler** ^12.0.1 - İzin yönetimi
- **shared_preferences** ^2.5.3 - Yerel veri depolama
- **flutter_iconpicker** ^4.0.1 - İkon seçme
- **http** ^1.0.0 - HTTP istekleri

## 📁 Proje Yapısı

```
lib/
├── 📂 l10n/                     # Çoklu dil desteği
│   ├── app_en.arb              # İngilizce çeviriler
│   ├── app_tr.arb              # Türkçe çeviriler
│   ├── app_de.arb              # Almanca çeviriler
│   └── app_localizations.dart   # Lokalizasyon yöneticisi
├── 📂 layouts/                  # Layout bileşenleri
│   └── main_layout.dart        # Ana uygulama düzeni
├── 📂 providers/                # State management
│   ├── theme_provider.dart     # Tema yöneticisi
│   └── locale_provider.dart    # Dil yöneticisi
├── 📂 screens/                  # Uygulama ekranları
│   ├── auth_screen.dart        # Giriş/Kayıt ekranı
│   ├── home_screen.dart        # Ana sayfa
│   ├── forgot_password.dart    # Şifre sıfırlama
│   ├── user_agreement_page.dart # Kullanıcı sözleşmesi
│   ├── 📂 main/                # Ana uygulama ekranları
│   │   ├── main_screen.dart    # Ana ekran
│   │   ├── digital_business_card.dart # Kartvizit görüntüleme
│   │   ├── my_business_cards.dart # Kartvizit yönetimi
│   │   ├── scan_screen.dart    # QR kod tarayıcı
│   │   ├── profile_screen.dart # Profil yönetimi
│   │   └── settings_screen.dart # Ayarlar
│   └── 📂 cards/               # Kartvizit tasarımları
│       ├── card_1.dart         # Kartvizit şablonu 1
│       └── card_2.dart         # Kartvizit şablonu 2
├── 📂 widgets/                  # Özel widget'lar
│   ├── fade_in_wrapper.dart    # Animasyon wrapper'ı
│   ├── login_form.dart         # Giriş formu
│   ├── register_form.dart      # Kayıt formu
│   ├── forgot_password_form.dart # Şifre sıfırlama formu
│   ├── otp_input.dart          # OTP girişi (şuan kullanılmıyor)
│   └── profile_sheet.dart      # Profil bottom sheet'i
├── 📂 utils/                   # Yardımcı fonksiyonlar
│   ├── constants.dart          # Sabitler
│   ├── alert.dart              # Alert dialogs
│   └── get_icon_data.dart      # Icon yardımcıları
└── main.dart                   # Uygulama giriş noktası
```

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- Flutter SDK ^3.7.2
- Dart SDK
- Android Studio / VS Code
- Git

### Adım Adım Kurulum

1. **Projeyi klonlayın**
```bash
git clone https://github.com/Octapull/digital-business-card.git
cd digital-business-card
```

2. **Dependencies'leri yükleyin**
```bash
flutter pub get
```

3. **Lokalizasyon dosyalarını oluşturun**
```bash
flutter gen-l10n
```

4. **Uygulamayı çalıştırın**
```bash
flutter run
```

### Platform Specific Setup

#### Android
- Minimum SDK: 21
- Target SDK: 34
- Kamera izni gerekli (QR kod tarama için)

#### iOS
- Minimum iOS: 12.0
- Kamera kullanım izni gerekli
- Info.plist konfigürasyonu yapılmalı

## 🎯 Kullanım

### Temel Kullanım Akışı

1. **Hesap Oluşturma**: Uygulamayı açtıktan sonra "Kayıt Ol" sekmesinden hesap oluşturun
2. **Profil Tamamlama**: Kişisel bilgilerinizi doldurun
3. **Kartvizit Oluşturma**: İstediğiniz tasarımı seçerek kartvizitinizi oluşturun
4. **QR Kod Paylaşımı**: Oluşturulan QR kodu ile bilgilerinizi paylaşın
5. **Tarama**: Diğer kullanıcıların QR kodlarını tarayarak bilgilerine erişin

### Özellik Kullanımları

#### 🎨 Tema Değiştirme
1. Ayarlar → Uygulama Ayarları → Karanlık/Açık Mod

#### 🌍 Dil Değiştirme
1. Ayarlar → Uygulama Ayarları → Dil

#### 📱 QR Kod Tarama
1. Alt menüden "Tara" sekmesini seçin
2. Kamera izni verin
3. QR kodu kare içine hizalayın

## 🔧 Konfigürasyon

### Tema Ayarları
```dart
// Theme Provider kullanımı
final themeProvider = context.watch<ThemeProvider>();
themeProvider.setThemeMode(ThemeMode.dark);
```

### Lokalizasyon Ayarları
```dart
// Locale Provider kullanımı
final localeProvider = context.watch<LocaleProvider>();
localeProvider.setLocale(Locale('tr'));
```

### Geliştirme Kuralları
- Clean code prensiplerine uyun
- Widget'ları modüler yapıda geliştirin
- Yorum satırları ekleyin
- Test yazın

## 👥 Takım

- **Geliştirici**: Özgür Yurt
- **Tasarım**: Özgür Yurt
- **Proje Yöneticisi**: Mustafa Ergeç, Emre Gündoğdu

## 📞 İletişim

- **E-posta**: ozgurryurtt@gmail.com
- **LinkedIn**: [[LinkedIn Profili]](https://www.linkedin.com/in/ozguryurt)
- **GitHub**: [[GitHub Profili]](https://github.com/ozguryurt)

## 🎉 Teşekkürler

- Flutter takımına harika framework için
- ForUI takımına modern UI bileşenleri için
- Açık kaynak topluluğuna katkıları için

---

<div align="center">
  <p>❤️ ile Flutter ile geliştirilmiştir</p>
</div>
