// src/app/components/profile/profile.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

import { ProfileService } from '../../services/profile';
import { UsersService, UserPreferences } from '../../services/users';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

import { LinkEditor } from '../link-editor/link-editor';
import { CropperDialogComponent, CropperDialogData, CropperDialogResult } from './cropper-dialog/cropper-dialog.component';

import { UserProfile } from '../../models/user-profile.model';
import { User } from '../../models/user.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LinkEditor,
    QRCodeComponent,
    FormsModule,
    CropperDialogComponent
  ],
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss']
})
export class Profile implements OnInit {
  userId!: string;
  user?: User;
  profile?: UserProfile;
  showQr = false;
  showCropper = false;
  showSettings = false;
  showEditMode = false;
  showPhotoActions = false;
  canEdit = false;
  sessionExpired = false;
  accessDenied = false;
  selectedLang: 'tr' | 'en' = 'tr';
  imageBaseUrl = 'http://localhost:5078';
  cropperData?: CropperDialogData;
  // Avatar rendering control to avoid default flash
  profileSectionReady = false; // becomes true after user+profile fetch attempt
  avatarLoaded = false;
  avatarFallbackReady = false; // when we allow showing default after timeout
  photoCacheBuster = Date.now();
  
  
  // Link display preference
  linkViewMode: 'list' | 'grid' = 'list';
  gridColumns: number = 3; // Default 3 columns, range 3-5
  
  // Mode management - simplified (link-editor handles its own modes)
  showQrCode = false;

  // Modal for content display
  showContentModal = false;
  selectedContent = { title: '', value: '', type: 'text' };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
  private profSvc: ProfileService,
  private userSvc: UsersService,
  private auth: AuthService,
  private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('userId');
      if (!id) return;
      this.userId = id;
      
      this.validateAccess();
      // Token kontrolü auth service constructor ve auth interceptor'da yapılıyor
    });
  }

  get profileUrl(): string {
    return `${window.location.origin}/profil/${this.userId}`;
  }

  private validateAccess(): void {
    const token = this.auth.getToken();
    if (!token) { this.router.navigate(['/login']); return; }
    this.auth.validateToken(token, this.userId).subscribe({
      next: res => {
        if (!res.success) return;
        this.canEdit = !!res.canEdit;
        this.loadDataWithPrivacy();
      },
      error: err => {
        // Auth interceptor timeout'ları halledecek, buradan kaldırıyoruz
        this.router.navigate(['/login']);
      }
    });
  }

  onLangChange(event: any) {
    this.selectedLang = event.target.value;
    // Burada ileride i18n desteği eklenebilir
  }

  private loadDataWithPrivacy(): void {
    this.userSvc.getById(this.userId).subscribe({
      next: u => {
        this.user = u;
        // Backend'den preferences'ları da çek
        this.userSvc.getPreferences(this.userId).subscribe({
          next: (prefs: UserPreferences) => {
            // Preferences'ları uygula
            this.linkViewMode = prefs.viewMode === 'grid' ? 'grid' : 'list';
            this.gridColumns = prefs.gridColumns || 3;
          },
          error: () => {
            // Default values kalır
            console.log('Preferences yüklenemedi, default değerler kullanılıyor');
          }
        });
        
        // IsPublic artık UserPreferences tablosunda - backend'de kontrol ediliyor
        // Frontend'te sadece canEdit kontrolü yeterli
        this.profSvc.get(this.userId).subscribe({
          next: p => {
            this.profile = p;
            this.profileSectionReady = true;
            // If no custom photo, allow default after a tiny delay to avoid layout shift
            if (!p?.profilePhotoUrl) {
              setTimeout(() => { this.avatarFallbackReady = true; }, 30);
            }
          },
          error: _ => {
            // Profile erişimi reddedildi - muhtemelen private profil
            this.accessDenied = true;
            this.profileSectionReady = true;
          }
        });
      },
      error: _ => {
        this.accessDenied = true;
        this.profileSectionReady = true;
      }
    });
  }

  get sortedLinks() {
    // sortId'ye göre sıralı array döndür
    return (this.profile?.links ?? []).slice().sort((a, b) => (a.sortId ?? 0) - (b.sortId ?? 0));
  }

  get filteredLinks() {
    // Dinamik bağlantılardan üstte gösterilen alanları filtrele
    const excluded = [
      'e-mail','email','mail','telefon','phone','full name','fullname','ad','ad soyad','isim','company','şirket'
    ];
    return this.sortedLinks.filter(link => !excluded.includes(link.definitionName?.toLowerCase?.() || ''));
  }

  onLinksChanged = () => {
  this.loadDataWithPrivacy();
  }

  onPhotoSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;
    
    // Check file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
      alert('Lütfen sadece JPEG, PNG veya GIF formatında resim yükleyin.');
      return;
    }

    // Check file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan küçük olmalıdır.');
      return;
    }

    // Open cropper dialog
    this.cropperData = {
      image: file,
      width: 120,
      height: 150
    };
    this.showCropper = true;
  }

  onCropperResult(result: CropperDialogResult | undefined) {
    this.showCropper = false;
    this.cropperData = undefined;
    
    if (result) {
      this.uploadCroppedImage(result.blob);
    }
  }

  uploadCroppedImage(blob: Blob) {
    const formData = new FormData();
    formData.append('file', blob, 'profile.jpg');
    
      this.profSvc.uploadPhoto(this.userId, formData).subscribe({
      next: () => {
        // Refresh only photo info; avoid full reload
        this.profSvc.get(this.userId).subscribe(p => {
          if (this.profile) {
            this.profile.profilePhotoUrl = p.profilePhotoUrl;
          } else {
            this.profile = p;
          }
          this.avatarLoaded = false;
          this.photoCacheBuster = Date.now();
          // allow image element to re-bind
          setTimeout(() => {
            // If no photo returned, enable fallback
            if (!this.profile?.profilePhotoUrl) {
              this.avatarFallbackReady = true;
            }
          });
        });
      },
      error: err => alert('Fotoğraf yüklenemedi: ' + (err?.error?.message || err.message))
    });
  }

  onDeletePhoto() {
    if (!this.profile?.profilePhotoUrl) return;
    
    if (!confirm('Profil fotoğrafını silmek istediğinizden emin misiniz?')) {
      return;
    }

    this.profSvc.deletePhoto(this.userId).subscribe({
      next: () => {
        // Just clear local state & show fallback without full reload
        if (this.profile) {
          this.profile.profilePhotoUrl = '';
        }
        this.avatarLoaded = false;
        this.avatarFallbackReady = true;
        this.photoCacheBuster = Date.now();
        // trigger fade-in of fallback
        setTimeout(() => { this.avatarLoaded = true; }, 30);
      },
      error: err => alert('Fotoğraf silinemedi: ' + (err?.error?.message || err.message))
    });
  }

  shareProfile() {
    const profileUrl = `${window.location.origin}/profil/${this.userId}`;
    
    if (navigator.share) {
      navigator.share({
        title: `${this.user?.fullName} - Diji-Card Profili`,
        text: `${this.user?.fullName} adlı kullanıcının dijital kartvizitini görüntüleyin`,
        url: profileUrl
      });
    } else {
      // Fallback: Clipboard'a kopyala
      navigator.clipboard.writeText(profileUrl).then(() => {
        alert('Profil bağlantısı panoya kopyalandı!');
      }).catch(() => {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = profileUrl;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        alert('Profil bağlantısı panoya kopyalandı!');
      });
    }
  }

  logout() {
    this.auth.logout();
  }

  getLinkIcon(definitionName: string): string {
    const name = definitionName.toLowerCase();
    if (name.includes('mail')) return 'fas fa-envelope';
    if (name.includes('tel') || name.includes('phone')) return 'fas fa-phone';
    if (name.includes('linkedin')) return 'fab fa-linkedin';
    if (name.includes('whatsapp')) return 'fab fa-whatsapp';
    if (name.includes('instagram')) return 'fab fa-instagram';
    if (name.includes('twitter') || name.includes('x.com')) return 'fab fa-x-twitter';
    if (name.includes('facebook')) return 'fab fa-facebook';
    if (name.includes('github')) return 'fab fa-github';
    if (name.includes('website') || name.includes('web')) return 'fas fa-globe';
    if (name.includes('iban') || name.includes('bank')) return 'fas fa-university';
    if (name.includes('address') || name.includes('adres') || name.includes('location')) return 'fas fa-map-marker-alt';
    if (name.includes('youtube')) return 'fab fa-youtube';
    if (name.includes('tiktok')) return 'fab fa-tiktok';
    return 'fas fa-link';
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
    if (!this.showSettings) {
      this.showEditMode = false;
    }
  }

  // Edit Mode Management - simplified (link-editor handles its own modes)
  toggleEditMode(): void {
    this.showEditMode = !this.showEditMode;
    this.showSettings = false; // Settings menüsünü kapat
  }

  // Photo Management
  togglePhotoActions(): void {
    this.showPhotoActions = !this.showPhotoActions;
  }

  navigateToSearch(): void {
    this.router.navigate(['/search']);
  }

  goToMyProfile(): void {
    // Auth service'ten kendi userId'mizi alıp oraya yönlendir
    const token = this.auth.getToken();
    if (token) {
      // Token'dan userId'yi parse et
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log('Token payload:', payload); // Debug için
        
        // Farklı claim isimlerini dene
        const myUserId = payload.userId || payload.uid || payload.sub || payload.nameid;
        console.log('Found userId:', myUserId, 'Current userId:', this.userId); // Debug için
        
        if (myUserId && myUserId !== this.userId) {
          console.log('Navigating to my profile:', myUserId); // Debug için
          this.router.navigate(['/profil', myUserId]);
        } else if (myUserId === this.userId) {
          console.log('Already on my profile'); // Debug için
        } else {
          console.error('Token\'da userId bulunamadı:', payload);
          alert('Kullanıcı bilgisi bulunamadı. Lütfen tekrar giriş yapın.');
          this.router.navigate(['/login']);
        }
      } catch (error) {
        console.error('Token parse hatası:', error);
        alert('Token hatası. Lütfen tekrar giriş yapın.');
        this.router.navigate(['/login']);
      }
    } else {
      console.error('Token bulunamadı');
      alert('Oturum bulunamadı. Lütfen giriş yapın.');
      this.router.navigate(['/login']);
    }
  }

  // Avatar load handler to fade-in
  onAvatarLoad(): void {
    this.avatarLoaded = true;
  }

  toggleLinkView(): void {
    this.linkViewMode = this.linkViewMode === 'list' ? 'grid' : 'list';
    
    // Backend'e preferences güncelleme
    if (this.canEdit) {
      const preferences: UserPreferences = {
        viewMode: this.linkViewMode
      };
      this.userSvc.updatePreferences(this.userId, preferences).subscribe({
        next: () => {
          console.log('View mode updated to:', this.linkViewMode);
        },
        error: (err) => {
          console.error('Failed to update view mode:', err);
        }
      });
    }
  }

  changeGridColumns(columns: number): void {
    if (columns >= 3 && columns <= 5) {
      this.gridColumns = columns;
      
      // Backend'e preferences güncelleme
      if (this.canEdit) {
        const preferences: UserPreferences = {
          gridColumns: columns
        };
        this.userSvc.updatePreferences(this.userId, preferences).subscribe({
          next: () => {
            console.log('Grid columns updated to:', columns);
          },
          error: (err) => {
            console.error('Failed to update grid columns:', err);
          }
        });
      }
    }
  }

  onLinkClick(link: any): void {
    if (this.linkViewMode === 'grid') {
      // Grid görünümünde tıklama davranışı
      if (link.value.startsWith('http')) {
        // Link ise direkt yönlendir
        window.open(link.value, '_blank');
      } else if (link.value.startsWith('mailto:')) {
        // Email ise mail uygulamasını aç
        window.location.href = link.value;
      } else if (link.value.startsWith('tel:')) {
        // Telefon ise arama yap
        window.location.href = link.value;
      } else {
        // Diğer durumlarda modal açıp içeriği göster
        this.selectedContent = {
          title: link.definitionName,
          value: link.value,
          type: this.getContentType(link.definitionName)
        };
        this.showContentModal = true;
      }
    }
    // List görünümünde click eventi işlenmez, direkt link çalışır
  }

  // Handle both left click and right click for links in grid view
  handleLinkClick(event: MouseEvent, link: any): void {
    // Sol tık: Normal link davranışı (href çalışır)
    // Sağ tık: Browser context menu çalışır
    
    // Eğer sol tık ise ve IBAN/Adres gibi özel içerik ise modal aç
    if (event.button === 0) { // Sol tık
      if (!link.value.startsWith('http') && !link.value.startsWith('mailto:') && !link.value.startsWith('tel:')) {
        // IBAN, Adres vb. özel içerikler için modal aç
        event.preventDefault(); // Link'in default davranışını engelle
        this.selectedContent = {
          title: link.definitionName,
          value: link.value,
          type: this.getContentType(link.definitionName)
        };
        this.showContentModal = true;
      }
      // HTTP, mailto, tel linkleri için href doğal olarak çalışır (preventDefault yok)
    }
    // Sağ tık (button !== 0) için hiçbir şey yapmayız, browser context menu çalışır
  }

  getContentType(definitionName: string): string {
    const name = definitionName.toLowerCase();
    if (name.includes('iban') || name.includes('bank')) return 'iban';
    if (name.includes('address') || name.includes('adres')) return 'address';
    return 'text';
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.notificationService.showToast('Panoya kopyalandı!', 'success');
    }).catch(() => {
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = text;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
      this.notificationService.showToast('Panoya kopyalandı!', 'success');
    });
  }

  closeContentModal(): void {
    this.showContentModal = false;
  }

  openInMaps(address: string): void {
    const url = `https://maps.google.com/?q=${encodeURIComponent(address)}`;
    window.open(url, '_blank');
  }

  private startTokenExpiryCheck(): void {
    // Test için 10 saniyede bir kontrol et
    setInterval(() => {
      if (this.auth.isTokenExpired()) {
        this.sessionExpired = true;
        console.log('Token expired, showing session timeout popup');
      }
    }, 10000); // 10 saniye (test için)
  }

  goToLogin(): void {
    // Token'ları temizle ve login'e git
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
    this.router.navigate(['/login']);
  }

}
