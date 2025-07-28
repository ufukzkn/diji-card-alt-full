// src/app/components/profile/profile.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
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
  selectedLang: 'tr' | 'en' = 'tr';
  imageBaseUrl = 'https://localhost:7220';
  cropperData?: CropperDialogData;

  constructor(
    private route: ActivatedRoute,
    private profSvc: ProfileService,
    private userSvc: UsersService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('userId');
      if (!id) return;
      this.userId = id;
      this.loadData();
    });
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

  get profileUrl() {
    return window.location.origin + '/profile/' + this.userId;
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
}
