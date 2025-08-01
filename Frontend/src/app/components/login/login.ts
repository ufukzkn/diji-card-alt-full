import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest, OAuthTokenRequest } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  loginData: LoginRequest = {
    kullaniciAdi: '',
    sifre: '',
    authCode: '',
    applicationId: '01C257EF-C9B4-446D-A45A-761D5B93CC97',
    redirectUrl: '',
    requestId: '',
    source: 'web',
    isShowExceptionMessage: false
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogin(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // AuthCode'u kullanıcı adı ile aynı yap
    this.loginData.authCode = this.loginData.kullaniciAdi;
    this.loginData.requestId = this.generateRequestId();

    this.authService.kullaniciGirisYap(this.loginData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Giriş başarılı! Token alınıyor...';
          
          // OAuth token almak için ikinci endpoint'i çağır
          const tokenRequest: OAuthTokenRequest = {
            requestId: response.requestId,
            authToken: response.authToken,
            redirectUrl: response.redirectUrl || ''
          };

          this.authService.oAuthToken(tokenRequest).subscribe({
            next: (tokenResponse) => {
              if (tokenResponse.success) {
                this.authService.saveToken(tokenResponse);
                this.successMessage = 'Giriş başarılı! Profilinize yönlendiriliyorsunuz...';
                
                // Token'dan userId'yi çıkar
                const userId = this.getUserIdFromToken(tokenResponse.accessToken);
                
                // Kullanıcının kendi profiline yönlendir
                const redirectPath = userId ? `/profil/${userId}` : '/search';
                
                setTimeout(() => {
                  this.router.navigate([redirectPath]);
                }, 1500);
              } else {
                this.errorMessage = tokenResponse.message || 'Token alırken hata oluştu.';
              }
              this.isLoading = false;
            },
            error: (error) => {
              this.errorMessage = 'Token alırken hata oluştu.';
              this.isLoading = false;
              console.error('Token error:', error);
            }
          });
        } else {
          this.errorMessage = response.message || 'Giriş başarısız.';
          this.isLoading = false;
        }
      },
      error: (error) => {
        this.errorMessage = 'Giriş sırasında hata oluştu.';
        this.isLoading = false;
        console.error('Login error:', error);
      }
    });
  }

  private generateRequestId(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  private getUserIdFromToken(token: string): string | null {
    try {
      // Base64 decode token (new format: "userId:expires:timestamp...")
      const decoded = atob(token);
      const parts = decoded.split(':');
      if (parts.length >= 1) {
        return parts[0]; // userId part
      }
      return null;
    } catch (error) {
      console.error('Token decode error:', error);
      return null;
    }
  }
}