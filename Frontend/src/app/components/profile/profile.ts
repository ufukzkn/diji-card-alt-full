// src/app/components/profile/profile.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

import { ProfileService } from '../../services/profile';
import { UsersService } from '../../services/users';

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
  selectedLang: 'tr' | 'en' = 'tr';
  imageBaseUrl = 'http://localhost:5078';
  cropperData?: CropperDialogData;
  
  // Mode management - simplified (link-editor handles its own modes)
  showQrCode = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private profSvc: ProfileService,
    private userSvc: UsersService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('userId');
      if (!id) return;
      this.userId = id;
      
      // Token kontrolü - sadece kendi profiline erişebilir
      this.checkOwnership();
      this.loadData();
    });
  }

  get profileUrl(): string {
    return `${window.location.origin}/profil/${this.userId}`;
  }

  private checkOwnership(): void {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    try {
      const decoded = atob(token);
      const tokenUserId = decoded.split(':')[0];
      
      if (tokenUserId !== this.userId) {
        alert('Bu profile erişim yetkiniz yok!');
        this.router.navigate(['/search']);
        return;
      }
    } catch (error) {
      this.router.navigate(['/login']);
    }
  }

  onLangChange(event: any) {
    this.selectedLang = event.target.value;
    // Burada ileride i18n desteği eklenebilir
  }

  private loadData(): void {
    // Default user info from Users table
    this.userSvc.getById(this.userId).subscribe(u => (this.user = u));

    // Dynamic links from UserDefinitionValues
    this.profSvc.get(this.userId).subscribe(p => (this.profile = p));
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
    this.loadData();
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
        this.loadData();
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
        this.loadData();
        // Sayfayı yenile
        window.location.reload();
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
}
