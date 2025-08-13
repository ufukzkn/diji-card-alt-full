# API Dokümantasyonu

Bu doküman backend API mimarisinin ve kaynaklarının özetidir.

## Kimlik Doğrulama Akışı

1. `POST /api/auth/KullaniciGirisYap` kullanıcı adı / şifre doğrular. Response: `RequestId`, `AuthToken`.
2. `POST /api/auth/OAuthToken` (RequestId + AuthToken) -> `AccessToken` (10 dk geçerli) + Refresh için placeholder (şimdilik yok).
3. Her profil / edit öncesi (isteğe bağlı) `POST /api/auth/ValidateToken` çağrısı: `CanEdit` (RequestedUserId == TokenUserId) döner.
4. Süre dolduysa mesaj: `Oturumunuz zaman aşımına uğradı` ve istemci logout / yönlendirme yapar.

> NOT: Mevcut AccessToken basit Base64 payload. Üretim ortamında gerçek JWT (imzalı) kullanılmalı. (TODO)

## Kaynak: UserDefinitionValues

| Metod | Yol | Amaç |
|-------|-----|------|
| GET | /api/userdefinitionvalues/{userId} | Kullanıcının tüm linkleri |
| POST | /api/userdefinitionvalues | Yeni regular veya custom link (DefinitionId=11 custom) |
| POST | /api/userdefinitionvalues/custom | Custom ekleme kısayolu |
| PUT | /api/userdefinitionvalues/byid/{id} | ID ile value güncelle (light DTO) |
| PUT | /api/userdefinitionvalues/{userId}/{definitionId} | Eski composite key update (backward) |
| DELETE | /api/userdefinitionvalues/{id} | ID ile sil |
| PUT | /api/userdefinitionvalues/sort | Toplu sortId güncelle |

### Veri Kuralları

- Regular (DefinitionId != 11): `CustomDefinitionName` back-end tarafından NULL zorlanır.
- Custom (DefinitionId = 11): `CustomDefinitionName` zorunlu, trim'lenir, boş veya `[default]` olamaz.
- Unique index: (UserId, DefinitionId) sadece regular kayıtlar için.
- SortId UI tarafında sıraya göre güncellenir.

### Ziyaret Loglama (Yeni)

- `ProfileVisit` tablosu ile profil görüntülemeleri kaydedilir: `ProfileUserId`, optional `VisitorUserId`, zaman damgası.
- IP ve UserAgent kişisel veri saklamamak için hash'lenebilir (TODO: Hash helper ekle).
- Sorgu örneği (son 30 gün ziyaret sayısı):

```sql
SELECT COUNT(*) FROM "ProfileVisits" 
WHERE "ProfileUserId" = @userId 
	AND "VisitedAtUtc" > NOW() - INTERVAL '30 days';
```

## Kaynak: Profile

| Metod | Yol | Amaç |
|-------|-----|------|
| GET | /api/profile/{userId} | Kullanıcı profili + dinamik linkler |
| POST | /api/profile/{userId}/photo | Profil fotoğrafı yükle |
| DELETE | /api/profile/{userId}/photo | Profil fotoğrafı sil |
| GET | /api/profile/{userId}/custom-definitions | Kullanıcının custom link kayıtları |
| GET | /api/profile/{userId}/custom-definition-names | Sadece custom tanım adları |

Gelecek (TODO):
- `GET /api/profile/{userId}/visits` -> ziyaret istatistikleri
- `GET /api/profile/{userId}/visits/daily?days=30` -> zaman serisi

## Gizlilik / Görünürlük

- User.IsPublic = false ise profil sadece sahibi tarafından görülebilir (UI + ileride controller guard eklenecek).
- Arama / listeleme fonksiyonları IsPublic = true filtrelemeli (TODO: Search endpoint eklenecek).
- Parola korumalı veya davet kodu yaklaşımı için olası alanlar: `AccessCode`, `InvitationOnly` (henüz eklenmedi).

## Yetki Mantığı

- `CanEdit` yalnızca ValidateToken sonucu true dönüyorsa UI’da edit butonları aktif.
- Server-side enforcement (TODO): Mutasyon endpoint'lerinde AccessToken decode edilip kullanıcı eşleştirilecek.

## Güncelleme / Genişletme Planı (Backlog)

| Başlık | Durum | Not |
|--------|-------|-----|
| JWT Auth | TODO | Gerçek imzalı token, refresh flow |
| PATCH endpoints | TODO | Kısmî güncelleme için (value vs) |
| Rate limiting | TODO | Auth brute force koruması |
| Profile visits API | TODO | Raporlama endpoint'leri |
| AccessCode / Davet | TODO | Özel paylaşım modları |

## Örnek ValidateToken Response

```json
{
	"success": true,
	"message": "Token geçerli",
	"userId": "user1",
	"expiresAt": "2025-08-12T12:34:56Z",
	"canEdit": true
}
```

Süre dolmuş örnek:

```json
{ "success": false, "message": "Oturumunuz zaman aşımına uğradı" }
```

---
Bu doküman taslaktır; geliştirme ilerledikçe güncellenecektir.
