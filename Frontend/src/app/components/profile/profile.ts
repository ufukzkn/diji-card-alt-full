// src/app/components/profile/profile.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { FormsModule } from '@angular/forms';

import { ProfileService } from '../../services/profile';
import { UsersService } from '../../services/users';

import { LinkEditor } from '../link-editor/link-editor';

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
    FormsModule
  ],
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss']
})
export class Profile implements OnInit {
  userId!: string;
  user?: User;
  profile?: UserProfile;
  showQr = false;
  selectedLang: 'tr' | 'en' = 'tr';

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
}
