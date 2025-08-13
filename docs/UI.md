# UI Dokümantasyonu (Taslak)

## Akış

1. Login -> RequestId + AuthToken
2. OAuthToken -> AccessToken
3. Profil sayfası açılışında: linkler + ValidateToken (CanEdit)
4. CanEdit true ise edit modları aktif: Add / Edit / Delete / Sort.

## Edit Yetkisi

- CanEdit false olduğunda link-editor komponentinde mod butonları gizlenir (TODO: Koşullu template gösterimi).

## Komponentler

- link-editor: CRUD + sıralama
- login: kimlik doğrulama formu
- profile: kullanıcı profil + link listesi render

## Timeout Davranışı

- ValidateToken dönerken süre dolmuşsa UI modal / toast: "Oturumunuz zaman aşımına uğradı" -> login'e yönlendir.
- (TODO) Global Http interceptor ile 401 / özel mesaj yakalanacak.

## Ekran Görselleri

(TODO: login.png)
(TODO: profile_edit.png)
(TODO: profile_view.png)

## Teknik Notlar

- Edit çağrıları ID üzerinden (`/byid/{id}`)
- Sort işlemi full liste gönderir (`PUT /sort`)

## TODO

- Global error interceptor
- Optimistic UI güncellemeleri
- Loading skeleton
- Dark tema
