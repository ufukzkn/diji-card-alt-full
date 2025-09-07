import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { LanguageSwitcherComponent } from '../shared/language-switcher/language-switcher.component';
import { ThemeToggleComponent } from '../shared/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-search',
  standalone: true,
  templateUrl: './search.html',
  styleUrls: ['./search.scss'],
  imports: [CommonModule, FormsModule, RouterModule, HttpClientModule, TranslocoModule, LanguageSwitcherComponent, ThemeToggleComponent]
})
export class Search implements OnInit {
  searchQuery = '';
  users: any[] = [];
  showAllResults = false;
  isLoading = false;
  showSettings = false;

  constructor(
    private http: HttpClient,
    private router: Router,
  private auth: AuthService,
  private t: TranslocoService
  ) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.http.get<any[]>('http://localhost:5078/api/user')
      .subscribe({
        next: (data) => {
          this.users = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error(this.t.translate('search.errors.loadUsers'), error);
          this.isLoading = false;
        }
      });
  }

  get filteredUsers(): any[] {
    const query = this.searchQuery.trim().toLowerCase();
    if (query.length < 3) return [];

    const results = this.users.filter(user =>
      user.fullName.toLowerCase().includes(query) ||
      user.jobTitle?.toLowerCase().includes(query) ||
      user.company?.toLowerCase().includes(query) ||
      user.email?.toLowerCase().includes(query)
    );

    return this.showAllResults ? results : results.slice(0, 3);
  }

  triggerSearch(): void {
    this.searchQuery = this.searchQuery.trim();
    if (this.searchQuery.length >= 3) {
      this.showAllResults = false; // Reset to show limited results first
    }
  }

  showMoreResults(): void {
    this.showAllResults = true;
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
  }

  goToMyProfile(): void {
    // Auth service'ten kendi userId'mizi alıp oraya yönlendir
    const token = this.auth.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log('Token payload:', payload); // Debug için
        
        // Farklı claim isimlerini dene
        const myUserId = payload.userId || payload.uid || payload.sub || payload.nameid;
        console.log('Found userId:', myUserId); // Debug için
        
        if (myUserId) {
          console.log('Navigating to profile:', myUserId); // Debug için
          this.router.navigate(['/profil', myUserId]);
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

  logout() {
    this.auth.logout();
  }
}
