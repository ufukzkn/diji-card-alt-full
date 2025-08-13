// src/app/components/profile/profile.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

import { ProfileService } from '../../services/profile';
import { UsersService } from '../../services/users';
import { AuthService } from '../../services/auth.service';

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
  
  // Mode management - simplified (link-editor handles its own modes)
  showQrCode = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
  private profSvc: ProfileService,
  private userSvc: UsersService,
  private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('userId');
      if (!id) return;
      this.userId = id;
      
  this.validateAccess();
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
        const msg = err?.error?.message || err?.error?.Message;
        if (msg && msg.includes('zaman aşımı')) {
          this.sessionExpired = true;
          setTimeout(() => this.logout(), 2500);
        } else {
          this.router.navigate(['/login']);
        }
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
        if (!u.isPublic && !this.canEdit) {
          this.accessDenied = true;
          this.profileSectionReady = true;
          return; // linkleri çekme
        }
        this.profSvc.get(this.userId).subscribe(p => {
          this.profile = p;
          this.profileSectionReady = true;
          // If no custom photo, allow default after a tiny delay to avoid layout shift
          if (!p?.profilePhotoUrl) {
            setTimeout(() => { this.avatarFallbackReady = true; }, 30);
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
    if (confirm('Çıkış yapmak istediğinizden emin misiniz?')) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tokenExpiry');
      this.router.navigate(['/login']);
    }
  }

  getLinkIcon(definitionName: string): string {
    const name = definitionName.toLowerCase();
    if (name.includes('mail')) return '📧';
    if (name.includes('tel') || name.includes('phone')) return '📞';
    if (name.includes('linkedin')) return '💼';
    if (name.includes('whatsapp')) return '💬';
    if (name.includes('instagram')) return '📷';
    if (name.includes('twitter')) return '🐦';
    if (name.includes('facebook')) return '📘';
    if (name.includes('website') || name.includes('web')) return '🌐';
    if (name.includes('iban') || name.includes('bank')) return '🏦';
    if (name.includes('address') || name.includes('adres')) return '📍';
    return '🔗';
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

  // Avatar load handler to fade-in
  onAvatarLoad(): void {
    this.avatarLoaded = true;
  }
}
