import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { LoginRequest, OAuthTokenRequest, LoginResponse, TokenResponse } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5078/api/Auth';
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);

  public token$ = this.tokenSubject.asObservable();
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private http: HttpClient) {
    // Sayfa yüklendiğinde localStorage'dan token'ı kontrol et
    const savedToken = localStorage.getItem('accessToken');
    if (savedToken) {
      this.tokenSubject.next(savedToken);
      this.isLoggedInSubject.next(true);
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

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
    
    this.tokenSubject.next(null);
    this.isLoggedInSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    const expiry = localStorage.getItem('tokenExpiry');
    
    if (!token || !expiry) {
      return false;
    }

    // Token'ın süresi dolmuş mu kontrol et
    const expiryDate = new Date(expiry);
    const now = new Date();
    
    if (now >= expiryDate) {
      this.logout();
      return false;
    }

    return true;
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
