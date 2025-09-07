import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject } from 'rxjs';
import { LoginRequest, OAuthTokenRequest, LoginResponse, TokenResponse } from '../models/auth.model';
import { NotificationService } from './notification.service';
import { TranslocoService } from '@jsverse/transloco';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5078/api/Auth';
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);

  public token$ = this.tokenSubject.asObservable();
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
  private notificationService: NotificationService,
  private t: TranslocoService
  ) {
    // Sayfa yüklendiğinde localStorage'dan token'ı kontrol et
    const savedToken = localStorage.getItem('accessToken');
    if (savedToken) {
      // Token varsa süresini kontrol et
      if (this.isTokenExpired()) {
        // Token süresi dolmuşsa temizle
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('tokenExpiry');
        this.tokenSubject.next(null);
        this.isLoggedInSubject.next(false);
      } else {
        // Token geçerliyse set et
        this.tokenSubject.next(savedToken);
        this.isLoggedInSubject.next(true);
      }
    }
  }

  kullaniciGirisYap(loginData: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/KullaniciGirisYap`, loginData);
  }

  oAuthToken(tokenData: OAuthTokenRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/OAuthToken`, tokenData);
  }

  validateToken(accessToken: string, requestedUserId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/ValidateToken`, {
      accessToken,
      requestedUserId
    });
  }

  saveToken(tokenResponse: TokenResponse): void {
    if (tokenResponse.success && tokenResponse.accessToken) {
      localStorage.setItem('accessToken', tokenResponse.accessToken);
      localStorage.setItem('refreshToken', tokenResponse.refreshToken);
      localStorage.setItem('tokenExpiry', tokenResponse.expiresAt);
      
      this.tokenSubject.next(tokenResponse.accessToken);
      this.isLoggedInSubject.next(true);
    }
  }

  // Global logout method with confirmation
  async logout(): Promise<void> {
    const confirmed = await this.notificationService.showConfirmation(
      'common.dialogs.logout.title',
      'common.dialogs.logout.message',
      'common.buttons.logout',
      'common.buttons.cancel'
    );
    
    if (confirmed) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tokenExpiry');
      
      this.tokenSubject.next(null);
      this.isLoggedInSubject.next(false);
      this.router.navigate(['/login']);
    }
  }

  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    return true; // Sadece token varlığını kontrol et, expiry kontrolü ayrı yapılacak
  }

  // Token süresini kontrol et ama logout yapma (popup için)
  isTokenExpired(): boolean {
    const expiry = localStorage.getItem('tokenExpiry');
    
    if (!expiry) {
      return true;
    }

    const expiryDate = new Date(expiry);
    const now = new Date();
    
    return now >= expiryDate;
  }

  // Token'ı decode et ve içeriğini döndür
  decodeToken(): any {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded = atob(token);
      console.log('Decoded Token:', decoded);
      return decoded;
    } catch (error) {
      console.error('Token decode error:', error);
      return null;
    }
  }

  // Token'dan kullanıcı bilgilerini çıkar
  getTokenInfo(): { userId?: string, expires?: string, timestamp?: string } | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;

    const parts = decoded.split(':');
    const info: any = {};
    
    for (let i = 0; i < parts.length; i += 2) {
      if (parts[i] && parts[i + 1]) {
        info[parts[i]] = parts[i + 1];
      }
    }
    
    return info;
  }
}
