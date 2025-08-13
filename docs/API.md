## API Dokümantasyonu (Taslak)

Bu dosya backend API mantığını açıklamak için hazırlanmıştır.

### Kimlik Doğrulama Akışı
1. `POST /api/auth/KullaniciGirisYap` kullanıcı adı/şifre ile giriş — RequestId + AuthToken döner.
2. `POST /api/auth/OAuthToken` AuthToken + RequestId doğrular, AccessToken (10 dk geçerli) döner.
3. Gerektikçe `POST /api/auth/ValidateToken` çağrısı ile geçerlilik ve `CanEdit` bilgisi alınır.
4. Süre dolmuşsa mesaj: `Oturumunuz zaman aşımına uğradı`.

### Kaynak: UserDefinitionValues
| Metod | Yol | Amaç |
|-------|-----|------|
| GET | /api/userdefinitionvalues/{userId} | Kullanıcının tüm linkleri |
| POST | /api/userdefinitionvalues | Yeni regular/custom link ekle (DefinitionId=11 ise custom) |
| POST | /api/userdefinitionvalues/custom | Kısayol: sadece custom ekleme |
| PUT | /api/userdefinitionvalues/byid/{id} | ID ile value güncelle (light DTO) |
| PUT | /api/userdefinitionvalues/{userId}/{definitionId} | Eski composite key update (geriye dönük) |
| DELETE | /api/userdefinitionvalues/{id} | ID ile sil |
| PUT | /api/userdefinitionvalues/sort | Toplu sortId güncelle |

### Veri Kuralları
- Regular (DefinitionId != 11): CustomDefinitionName daima NULL zorlanır.
- Custom (DefinitionId = 11): CustomDefinitionName zorunlu ve trim'lenir.
- '[default]' placeholder'ı reddedilir.
- Unique index (UserId, DefinitionId) sadece regular kayıtlar için.

### Güvenlik / Yetki
- `ValidateToken` sonucu `CanEdit = (RequestedUserId == TokenUserId)`.
- CanEdit false ise sadece GET işlemlerine izin verilecek (UI’da butonlar gizli). (Controller seviyesinde ek kısıtlama TODO.)

### TODO
- PATCH endpoint (kısmî güncelleme)
- Refresh token stratejisi
- Controller bazlı [Authorize] + gerçek JWT geçişi
