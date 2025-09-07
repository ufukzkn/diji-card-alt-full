// src/app/components/profile/profile.ts

import { Component, OnInit, AfterViewChecked, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

import { ProfileService } from '../../services/profile';
import { ProfileVisitService } from '../../services/profile-visit.service';
import { UsersService, UserPreferences } from '../../services/users';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

import { LinkEditor } from '../link-editor/link-editor';
import { CropperDialogComponent, CropperDialogData, CropperDialogResult } from './cropper-dialog/cropper-dialog.component';

import { UserProfile, PrivateProfileResponse, PrivateProfileAccessRequest, BasicUserInfo, FullProfileData } from '../../models/user-profile.model';
import { User } from '../../models/user.models';
import { LanguageService } from '../../services/language.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { LanguageSwitcherComponent } from '../shared/language-switcher/language-switcher.component';
import { ThemeToggleComponent } from '../shared/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LinkEditor,
    QRCodeComponent,
    FormsModule,
  CropperDialogComponent,
  TranslocoModule,
  LanguageSwitcherComponent,
  ThemeToggleComponent
  ],
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss']
})
export class Profile implements OnInit, AfterViewChecked {
  @ViewChild(LinkEditor) linkEditorComp?: LinkEditor;
  userId!: string;
  user?: User;
  profile?: UserProfile;
  basicInfo?: BasicUserInfo;
  showQr = false;
  showCropper = false;
  showSettings = false;
  showEditMode = false;
  showPhotoActions = false;
  canEdit = false;
  isLogged = false; // track logged in state for conditional UI
  sessionExpired = false;
  accessDenied = false;
  selectedLang: string = 'tr';
  langs = ['tr','en','de'];
  flags: Record<string,string> = { tr:'🇹🇷', en:'🇺🇸', de:'🇩🇪' };
  langLabels: Record<string,string> = { tr:'Türkçe', en:'English', de:'Deutsch' };
  langOpen = false;
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
  isSortMode = false;
  
  // Mode management - simplified (link-editor handles its own modes)
  showQrCode = false;

  // Modal for content display
  showContentModal = false;
  selectedContent = { title: '', value: '', type: 'text' };

  // Private profile system
  isPrivateProfile = false;
  accessGranted = false;
  privateAccessMessage = '';
  showInlinePassword = false; // replaces removed modal
  passwordInput = '';
  passwordError = '';

  // Privacy settings modal
  showPrivacySettings = false;
  privacyIsPublic = true;
  privacyPassword = '';
  specialLinks: any[] = [];
  showCreateLinkModal = false;
  newLinkDescription = '';
  newLinkExpiryDays = 7;
  // Manual expiry (datetime-local) optional override
  newLinkExpiryDateManual: string = '';

  enableManualExpiryFromPreset() {
    const base = new Date();
    base.setDate(base.getDate() + this.newLinkExpiryDays);
    // datetime-local expects yyyy-MM-ddTHH:mm
    const iso = base.toISOString();
    this.newLinkExpiryDateManual = iso.slice(0,16);
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
  private profSvc: ProfileService,
  private userSvc: UsersService,
  private auth: AuthService,
  private notificationService: NotificationService,
  private langService: LanguageService,
  private t: TranslocoService,
  private visitSvc: ProfileVisitService
  ) {
    // initialize selectedLang from service if stored
    this.selectedLang = this.langService.active || 'tr';
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('userId');
      if (!id) return;
      this.userId = id;

  // Ziyaret kaydı (tek sefer) - özel erişim veya normal erişimden bağımsız
  const isSpecial = !!this.route.snapshot.queryParamMap.get('access');
  this.logVisit(isSpecial);
      
      // Query parametrelerini kontrol et
      this.route.queryParams.subscribe(queryParams => {
        const accessToken = queryParams['access'];
        if (accessToken) {
          // Access token ile erişim
          this.validateSpecialAccess(accessToken);
        } else {
          // Normal JWT token ile erişim
          this.validateAccess();
        }
      });
    });
  }

  private visitLogged = false;

  private logVisit(isSpecialAccess: boolean) {
    if (this.visitLogged || !this.userId) return;
    this.visitLogged = true;
    this.visitSvc.logVisit(this.userId, isSpecialAccess).subscribe({
      next: () => { /* sessiz */ },
      error: err => console.warn('Visit log failed', err)
    });
  }

  get profileUrl(): string {
    // Şu anki URL'i döndür (access token dahil olabilir)
    return window.location.href;
  }

  private validateAccess(): void {
    const token = this.auth.getToken();
    if (!token) {
      // Anonymous view: just load privacy-aware data without edit rights
      this.isLogged = false;
      this.canEdit = false;
      this.loadDataWithPrivacy();
      return;
    }
    this.isLogged = true;
    this.auth.validateToken(token, this.userId).subscribe({
      next: res => {
        if (!res.success) {
          // Even if token invalid, fallback to anonymous view
          this.isLogged = false;
          this.canEdit = false;
          this.loadDataWithPrivacy();
          return;
        }
        this.canEdit = !!res.canEdit;
        this.loadDataWithPrivacy();
      },
      error: _err => {
        // On error treat as anonymous
        this.isLogged = false;
        this.canEdit = false;
        this.loadDataWithPrivacy();
      }
    });
  }

  private validateSpecialAccess(accessToken: string): void {
    // Access token ile direkt verify endpoint'ini çağır
    const request: PrivateProfileAccessRequest = {
      accessToken: accessToken
    };

    this.profSvc.verifyPrivateAccess(this.userId, request).subscribe({
      next: (response: PrivateProfileResponse) => {
        if (response.accessGranted && response.profileData) {
          // Access token ile erişim başarılı
          this.isPrivateProfile = !response.isPublic;
          this.accessGranted = true;
          this.profileSectionReady = true;
          this.canEdit = false; // Access token ile erişimde edit izni yok
          
          // Tam profil data'sı geldi mi kontrol et
          const profileData = response.profileData as any;
          if (profileData.links) {
            // Tam profil data'sı - hem basic info hem links
            this.profile = {
              userId: profileData.userId,
              fullName: profileData.fullName,
              company: profileData.company,
              email: profileData.email,
              phoneNumber: profileData.phoneNumber,
              profilePhotoUrl: profileData.profilePhotoUrl,
              links: profileData.links || []
            };
            this.basicInfo = {
              userId: profileData.userId,
              fullName: profileData.fullName,
              company: profileData.company,
              jobTitle: profileData.jobTitle,
              email: profileData.email,
              phoneNumber: profileData.phoneNumber,
              profilePhotoUrl: profileData.profilePhotoUrl
            };
          } else {
            // Sadece basic info
            this.basicInfo = response.profileData as BasicUserInfo;
          }
          
          // If no custom photo, allow default after a tiny delay to avoid layout shift
          if (!this.basicInfo?.profilePhotoUrl) {
            setTimeout(() => { this.avatarFallbackReady = true; }, 30);
          }

          // User preferences'ları da yükle (view mode için)
          this.loadUserPreferences();
        } else {
          // Erişim reddedildi
          // Don't mark accessDenied here; allow anonymous/private flow
          // Fallback to i18n key instead of hardcoded Turkish text
          this.privateAccessMessage = response.message || this.t.translate('profile.private.needPassword');
          this.isPrivateProfile = true;
          this.profileSectionReady = true;
        }
      },
      error: (err) => {
  console.error('Access token ile erişim hatası:', err);
  // On special access error treat as private instead of full denial
  this.isPrivateProfile = true;
  this.accessGranted = false;
  this.privateAccessMessage = this.t.translate('profile.private.invalidLink');
        this.isPrivateProfile = true;
        this.profileSectionReady = true;
      }
    });
  }

  // legacy method kept if template / other code still calls it
  changeLang(l: string) { /* handled by language switcher component now */ }

  private loadDataWithPrivacy(): void {
    // Önce basic info'yu güvenli endpoint ile çek
    this.profSvc.getBasicInfo(this.userId).subscribe({
      next: (response: PrivateProfileResponse) => {
        if (response.accessGranted && response.profileData) {
          // Erişim var, basic bilgileri göster
          this.basicInfo = response.profileData as BasicUserInfo;
          this.isPrivateProfile = !response.isPublic;
          this.accessGranted = true;
          this.profileSectionReady = true;
          
          // If no custom photo, allow default after a tiny delay to avoid layout shift
          if (!this.basicInfo?.profilePhotoUrl) {
            setTimeout(() => { this.avatarFallbackReady = true; }, 30);
          }

          // Eğer erişim varsa, links bilgilerini de çek
          if (response.accessGranted) {
            this.loadLinksData();
          }
        } else {
          // Erişim yok, private profil modal'ı göster
          this.isPrivateProfile = true;
          this.accessGranted = false;
          // Use backend message if localized; otherwise fallback to default i18n key
          this.privateAccessMessage = response.message || this.t.translate('profile.private.needPassword');
            this.showInlinePassword = false; // replaces removed modal
          this.profileSectionReady = true;
        }
      },
      error: _ => {
  // Hata durumunda dahi public gösterim denemesi: accessDenied yerine private kabul edip şifre isteyelim
  this.isPrivateProfile = true;
  this.accessGranted = false;
  this.privateAccessMessage = this.t.translate('profile.private.needPassword');
  this.profileSectionReady = true;
      }
    });

    // Preferences'ları yükle
    this.loadUserPreferences();
  }

  private loadUserPreferences(): void {
    // Preferences'ları da çek (edit mode için gerekli)
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
  }

  private loadLinksData(): void {
    // Links verilerini güvenli endpoint ile çek
    this.profSvc.getWithPrivacyCheck(this.userId).subscribe({
      next: (response: PrivateProfileResponse) => {
        if (response.accessGranted && response.profileData) {
          // Type check: eğer links property'si varsa UserProfile, yoksa BasicUserInfo
          if ('links' in response.profileData) {
            this.profile = response.profileData as UserProfile;
            console.log('Links yüklendi:', this.profile.links?.length || 0, 'adet');
          } else {
            console.log('Response UserProfile değil, BasicUserInfo:', response.profileData);
          }
        } else {
          console.log('Links erişimi reddedildi:', response.message);
        }
      },
      error: (err) => {
        console.error('Links yükleme hatası:', err);
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

  onLinkEditorModeChanged(mode: string) {
    this.isSortMode = (mode === 'sort');
  console.log('[Profile] modeChanged event alındı ->', mode, 'isSortMode:', this.isSortMode);
  }

  ngAfterViewChecked(): void {
    // Emitted event kaçarsa fallback senkronizasyonu
    const current = this.linkEditorComp?.mode === 'sort';
    if (current !== this.isSortMode) {
      this.isSortMode = current;
    }
  }


  onPhotoSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;
    
    // Check file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
  alert(this.t.translate('profile.msg.photo.invalidType'));
      return;
    }

    // Check file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
  alert(this.t.translate('profile.msg.photo.sizeExceeded'));
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
        this.profSvc.getBasicInfo(this.userId).subscribe(response => {
          if (response.accessGranted && response.profileData) {
            const basicInfo = response.profileData as BasicUserInfo;
            if (this.basicInfo) {
              this.basicInfo.profilePhotoUrl = basicInfo.profilePhotoUrl;
            } else {
              this.basicInfo = basicInfo;
            }
            this.avatarLoaded = false;
            this.photoCacheBuster = Date.now();
            // allow image element to re-bind
            setTimeout(() => {
              // If no photo returned, enable fallback
              if (!this.basicInfo?.profilePhotoUrl) {
                this.avatarFallbackReady = true;
              }
            });
          }
        });
      },
  error: err => alert(this.t.translate('profile.msg.photo.uploadFail') + ' ' + (err?.error?.message || err.message))
    });
  }

  async onDeletePhoto() {
    if (!this.basicInfo?.profilePhotoUrl) return;
    
  // Custom confirmation modal
  const ok = await this.notificationService.showConfirmation('common.dialogs.delete.title','profile.msg.photo.confirmDelete');
  if (!ok) return;

    this.profSvc.deletePhoto(this.userId).subscribe({
      next: () => {
        // Just clear local state & show fallback without full reload
        if (this.basicInfo) {
          this.basicInfo.profilePhotoUrl = '';
        }
        this.avatarLoaded = false;
        this.avatarFallbackReady = true;
        this.photoCacheBuster = Date.now();
        // trigger fade-in of fallback
        setTimeout(() => { this.avatarLoaded = true; }, 30);
      },
  error: err => alert(this.t.translate('profile.msg.photo.deleteFail') + ' ' + (err?.error?.message || err.message))
    });
  }

  shareProfile() {
    console.log('Share button clicked!'); // Debug
    const profileUrl = window.location.href; // Şu anki URL (access token dahil olabilir)
    
    // Local development'ta navigator.share çalışmayabilir, direkt clipboard'a kopyala
    if (navigator.share && window.location.protocol === 'https:') {
      console.log('Using navigator.share'); // Debug
      navigator.share({
  title: this.t.translate('profile.shareMeta.title', { name: this.basicInfo?.fullName }),
  text: this.t.translate('profile.shareMeta.text', { name: this.basicInfo?.fullName }),
        url: profileUrl
      }).catch(err => {
        console.log('Share failed:', err);
        this.copyToClipboardFallback(profileUrl);
      });
    } else {
      console.log('Using clipboard fallback'); // Debug
      this.copyToClipboardFallback(profileUrl);
    }
  }

  private copyToClipboardFallback(url: string) {
    // Clipboard API
    if (navigator.clipboard && window.isSecureContext) {
      navigator.clipboard.writeText(url).then(() => {
  this.notificationService.showToast(this.t.translate('profile.msg.link.copied'), 'success');
      }).catch(() => {
        this.oldSchoolCopy(url);
      });
    } else {
      this.oldSchoolCopy(url);
    }
  }

  private oldSchoolCopy(url: string) {
    // Fallback for older browsers
    const textArea = document.createElement('textarea');
    textArea.value = url;
    textArea.style.position = 'fixed';
    textArea.style.opacity = '0';
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand('copy');
    document.body.removeChild(textArea);
  this.notificationService.showToast(this.t.translate('profile.msg.link.copied'), 'success');
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
          alert(this.t.translate('common.errors.userNotFound'));
          this.router.navigate(['/login']);
        }
      } catch (error) {
        console.error('Token parse hatası:', error);
  alert(this.t.translate('common.errors.tokenParse'));
        this.router.navigate(['/login']);
      }
    } else {
      console.error('Token bulunamadı');
  alert(this.t.translate('common.errors.sessionMissing'));
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
  this.notificationService.showToast(this.t.translate('common.copied'), 'success');
    }).catch(() => {
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = text;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
  this.notificationService.showToast(this.t.translate('common.copied'), 'success');
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

  // Private profile methods
  submitPassword(): void {
    if (!this.passwordInput.trim()) {
  this.notificationService.showToast(this.t.translate('profile.msg.password.enter'), 'error');
      return;
    }

    const request: PrivateProfileAccessRequest = {
      authCode: '', // Token'dan gelecek
      password: this.passwordInput
    };

    // Yeni endpoint kullan - hem basic info hem links birlikte gelsin
    this.profSvc.verifyPrivateAccessFull(this.userId, request).subscribe({
      next: (response: PrivateProfileResponse) => {
        if (response.accessGranted && response.profileData) {
          // Şifre doğru, tam profil geldi (FullProfileData)
          const fullData = response.profileData as FullProfileData;
          
          // Basic bilgileri ayır
          this.basicInfo = {
            userId: fullData.userId,
            fullName: fullData.fullName,
            company: fullData.company,
            jobTitle: fullData.jobTitle,
            email: fullData.email,
            phoneNumber: fullData.phoneNumber,
            profilePhotoUrl: fullData.profilePhotoUrl
          };

          // Profile bilgilerini ayır
          this.profile = {
            userId: fullData.userId,
            links: fullData.links || [],
            profilePhotoUrl: fullData.profilePhotoUrl
          };
          
          this.accessGranted = true;
                this.showInlinePassword = false; // replaces removed modal
          this.passwordInput = '';
          this.notificationService.showToast(this.t.translate('profile.msg.access.granted'), 'success');
          
          // If no custom photo, allow default after a tiny delay to avoid layout shift
          if (!this.basicInfo?.profilePhotoUrl) {
            setTimeout(() => { this.avatarFallbackReady = true; }, 30);
          }

          console.log('Şifre ile tam profil yüklendi - Links:', this.profile?.links?.length || 0, 'adet');
        } else {
          // Şifre yanlış
          this.notificationService.showToast(response.message, 'error');
          this.passwordInput = '';
        }
      },
      error: (err) => {
  this.notificationService.showToast(this.t.translate('common.errors.generic') + ' ' + (err?.error?.message || err.message), 'error');
        this.passwordInput = '';
      }
    });
  }

  // Modal kapatma fonksiyonu kaldırıldı (artık inline giriş var)

  // Privacy Settings Methods
  openPrivacySettings(): void {
    this.showPrivacySettings = true;
    // Mevcut ayarları yükle
    this.privacyIsPublic = !this.isPrivateProfile;
    this.loadSpecialLinks();
  }

  closePrivacySettings(): void {
    this.showPrivacySettings = false;
    this.privacyPassword = '';
    this.specialLinks = [];
  }

  savePrivacySettings(): void {
    const settings: any = {
      isPublic: this.privacyIsPublic
    };

    // Sadece private profile ise password gönder
    if (!this.privacyIsPublic && this.privacyPassword) {
      settings.privateAccessPassword = this.privacyPassword;
    }
    // Public'e çekerken password'u hiç gönderme (mevcut değeri korunsun)

    this.profSvc.updatePrivacySettings(this.userId, settings).subscribe({
      next: () => {
  this.notificationService.showToast(this.t.translate('profile.msg.privacy.saved'), 'success');
        this.isPrivateProfile = !this.privacyIsPublic;
        this.closePrivacySettings();
      },
      error: (err) => {
  this.notificationService.showToast(this.t.translate('common.errors.prefix') + ' ' + (err?.error?.message || err.message), 'error');
      }
    });
  }

  loadSpecialLinks(): void {
    this.profSvc.getSpecialLinks(this.userId).subscribe({
      next: (links) => {
        this.specialLinks = links;
      },
      error: (err) => {
        console.error('Özel linkler yüklenemedi:', err);
      }
    });
  }

  openCreateLinkModal(): void {
    this.showCreateLinkModal = true;
    this.newLinkDescription = '';
    this.newLinkExpiryDays = 7;
  this.newLinkExpiryDateManual = '';
  }

  closeCreateLinkModal(): void {
    this.showCreateLinkModal = false;
  }

  createSpecialLink(): void {
    if (!this.newLinkDescription.trim()) {
  this.notificationService.showToast(this.t.translate('profile.msg.special.enterDesc'), 'error');
      return;
    }
    let expiryDate: Date;
    if (this.newLinkExpiryDateManual) {
      expiryDate = new Date(this.newLinkExpiryDateManual);
      const now = new Date();
      if (isNaN(expiryDate.getTime()) || expiryDate <= now) {
        this.notificationService.showToast(this.t.translate('profile.privacyModal.invalidManualDate') || 'Invalid expiry date', 'error');
        return;
      }
    } else {
      expiryDate = new Date();
      expiryDate.setDate(expiryDate.getDate() + this.newLinkExpiryDays);
    }

    const request = {
      description: this.newLinkDescription,
      expiryDate: expiryDate.toISOString()
    };

    this.profSvc.createSpecialLink(this.userId, request).subscribe({
      next: (response) => {
  this.notificationService.showToast(this.t.translate('profile.msg.special.created'), 'success');
        this.loadSpecialLinks();
        this.closeCreateLinkModal();
        
        // Linki panoya kopyala
        navigator.clipboard.writeText(response.specialUrl).then(() => {
          this.notificationService.showToast(this.t.translate('profile.msg.link.copied'), 'success');
        });
      },
      error: (err) => {
  this.notificationService.showToast(this.t.translate('common.errors.prefix') + ' ' + (err?.error?.message || err.message), 'error');
      }
    });
  }

  copySpecialLink(url: string): void {
    navigator.clipboard.writeText(url).then(() => {
  this.notificationService.showToast(this.t.translate('profile.msg.link.copied'), 'success');
    });
  }

  async deleteSpecialLink(linkId: number): Promise<void> {
    const ok = await this.notificationService.showConfirmation('common.dialogs.delete.title','profile.msg.special.confirmDelete');
    if (!ok) return;

    this.profSvc.deleteSpecialLink(this.userId, linkId).subscribe({
      next: () => {
  this.notificationService.showToast(this.t.translate('profile.msg.special.deleted'), 'success');
        this.loadSpecialLinks();
      },
      error: (err) => {
  this.notificationService.showToast(this.t.translate('common.errors.prefix') + ' ' + (err?.error?.message || err.message), 'error');
      }
    });
  }

}
