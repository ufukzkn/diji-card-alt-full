# Digital Business Card

**Digital Business Card**, kullanıcıların kendi dijital kartvizitlerini kolayca oluşturup paylaşmalarını sağlayan şık ve kullanıcı dostu bir web uygulamasıdır. Temel özellikleri şunlardır:

- **Profil Fotoğrafı Yükleme:** Kullanıcılar, kendilerine ait bir profil fotoğrafını yükleyerek kartvizitlerine kişisel bir dokunuş katabilir.  
- **Dijital Hesap Linkleri:** Sosyal medya (LinkedIn, Twitter, Instagram vb.), e-posta, web sitesi ve diğer dijital platform hesaplarını bir arada ekleyebilir; ziyaretçiler tek tıkla ilgili profillere yönlendirilebilir.  
- **Özelleştirilebilir Tasarım:** Renk şeması, yazı tipi ve düzen seçenekleri ile her kart vizit kullanıcıya özgü bir görünüm sunar.  
- **Kullanıcıya Özel URL:** Her kartvizite benzersiz bir kısa bağlantı (ör. `yourname.digitalcard.com/abc123`) atanır; bu sayede kolayca paylaşılabilir.  
- **QR Kod Desteği:** Kartvizit URL’si otomatik olarak oluşturulan QR kod ile birlikte sunulur; basılı materyallerde veya etkinliklerde taratılarak hızlı erişim sağlanır.  
- **Mobil Uyumluluk:** Tamamen responsive tasarımıyla hem masaüstü hem de mobil cihazlarda sorunsuz görüntülenir.  
- **Gizlilik ve Güvenlik:** Kullanıcı verileri güvenli bir şekilde saklanır; istenirse kartvizit yalnızca davet yoluyla veya parola korumalı olarak paylaşılabilir.  

Bu özellikler sayesinde, fiziksel kartvizitlere gerek kalmadan, profesyonel ve modern bir ortamda kendinizi ve iletişim bilgilerinizi etkili biçimde tanıtabilirsiniz.  

## Özellikler

- [ ] QR Link
- [ ] Özelleştirilebilir uniq link
- [ ] Sonsuz link vb. ekleme imkanı
- [ ] Profil ziyaretçi loglaması

## Gereksinimler

- .NET 8 SDK
- Node.js (v18+ önerilir)
- npm
- Angular CLI (v20+)
- PostgreSQL (veritabanı)
- Flutter (mobil uygulama için)

## Kurulum

1. **Backend (API) Kurulumu**
   - `dotnet restore` ile bağımlılıkları yükleyin.
   - PostgreSQL veritabanı oluşturun ve bağlantı ayarlarını `appsettings.json` dosyasında düzenleyin.
   - Gerekirse migration işlemlerini çalıştırın: `dotnet ef database update`

2. **Frontend (Angular) Kurulumu**
   - `cd Frontend`
   - `npm install` ile bağımlılıkları yükleyin.
   - `ng serve` ile Angular uygulamasını başlatın.

3. **Mobil (Flutter) Kurulumu**
   - Mobil kurulum ilgili klasörün Readme dosyasında mevcuttur.

## Kullanım

1. **Backend'i Başlatmak için:**
   - Ana dizinde: `dotnet run`

2. **Frontend'i Başlatmak için:**
   - `cd Frontend`
   - `ng serve`

3. **Mobil Uygulama:**
   - `cd src/mobile`
   - `flutter run`

Uygulama varsayılan olarak `http://localhost:5078` (backend) ve `http://localhost:4200` (frontend) adreslerinde çalışır.

## Proje Hiyerarşisi

```
digital-business-card/
├── Controllers/           # .NET API controller dosyaları
├── Data/                  # DbContext ve veri erişim katmanı
├── Models/                # Veri modelleri
├── Migrations/            # EF Core migration dosyaları
├── Frontend/              # Angular uygulaması
│   ├── src/app/components # Angular bileşenleri
│   ├── src/app/models     # Angular modelleri
│   ├── src/app/services   # Angular servisleri
├── wwwroot/               # Statik dosyalar (profil fotoğrafları vb.)
├── src/
│   ├── api/               # API ile ilgili dokümantasyon
│   ├── mobile/            # Flutter mobil uygulama
│   └── ui/                # UI ile ilgili dokümantasyon
├── appsettings.json       # API yapılandırma dosyası
├── Program.cs             # .NET giriş noktası
├── digital-business-card.csproj # Proje dosyası
└── README.md              # Proje açıklamaları
```

## Contributing

Fork the repository

Create a feature branch
git checkout -b feature/amazing-feature

Make your changes

Add tests for new functionality

Commit your changes
git commit -m 'Add amazing feature'

Push to the branch
git push origin feature/amazing-feature

Open a Pull Request
