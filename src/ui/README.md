# UI Dokümantasyonu

## Genel Akış

1. Login -> RequestId + AuthToken
2. OAuthToken -> AccessToken (10 dk)
3. Profil açılışı: linkler + ValidateToken (CanEdit)
4. CanEdit true ise edit modları (Add/Edit/Delete/Sort) aktif

## Edit Yetkisi

- `canEdit` false ise link-editor yalnızca görüntüleme modunda.
- Menü butonları *ngIf ile koşullu gösterilir.

## Komponentler

- link-editor: Link CRUD + sıralama (drag & drop)
- login: Giriş formu
- profile: Kullanıcı profil + link listesi

## Timeout Davranışı

- API yanıtı "Oturumunuz zaman aşımına uğradı" içeriyorsa:
	- Token temizle (memory / storage)
	- Kullanıcıya uyarı göster
	- Login rotasına yönlendir

## Ziyaret Loglama (Plan)

- Profil açılışında throttle edilmiş bir ziyaret kaydı gönderilecek (`POST /api/profile/{userId}/visit` - TODO)
- Anonim ise VisitorUserId null, IP ve UA hash'lenmiş formda gönderilebilir (opsiyonel)

## Gizlilik / Görünürlük

- User.IsPublic = false ve ziyaretçi owner değilse: "Bu profil gizli" mesajı + link listesi gizlenir.

## Teknik Notlar

- Edit: `/api/userdefinitionvalues/byid/{id}` sadece value günceller.
- Sort: Liste komple `/sort` endpoint'ine gönderilir.
- Custom ekleme: `POST /api/userdefinitionvalues/custom` (DefinitionId=11 sabit)

## Ekran Görselleri (TODO)

- (login.png)
- (profile_view.png)
- (profile_edit.png)

## Geliştirme Backlog

| Başlık | Durum |
|--------|-------|
| Global HTTP interceptor | TODO |
| Ziyaret log çağrısı | TODO |
| Dark tema | TODO |
| Loading skeleton | TODO |
| Gizli profil mesaj component | TODO |

---
Bu doküman taslaktır; geliştirme ilerledikçe güncellenecektir.
