import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { LanguageSwitcherComponent } from '../shared/language-switcher/language-switcher.component';
import { LoginRequest, OAuthTokenRequest } from '../../models/auth.model';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslocoModule, LanguageSwitcherComponent],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent implements OnInit {
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
    private router: Router,
    private route: ActivatedRoute,
  private notificationService: NotificationService,
  private t: TranslocoService
  ) {}

  ngOnInit(): void {
    // Timeout parametresi varsa kullanıcıyı bilgilendir
    this.route.queryParams.subscribe(params => {
      if (params['timeout'] === '1') {
        console.log('Timeout detected, showing toast...');
  this.notificationService.showToast(this.t.translate('session.timeout'), 'error');
      }
    });
  }

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
          this.successMessage = this.t.translate('login.msg.authSuccess');
          
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
                this.successMessage = this.t.translate('login.msg.redirect');
                
                // Token'dan userId'yi çıkar
                const userId = this.getUserIdFromToken(tokenResponse.accessToken);
                
                // Kullanıcının kendi profiline yönlendir
                const redirectPath = userId ? `/profil/${userId}` : '/search';
                
                setTimeout(() => {
                  this.router.navigate([redirectPath]);
                }, 1500);
              } else {
                this.errorMessage = tokenResponse.message || this.t.translate('common.errors.tokenFetch');
              }
              this.isLoading = false;
            },
            error: (error) => {
              this.errorMessage = this.t.translate('common.errors.tokenFetch');
              this.isLoading = false;
              console.error('Token error:', error);
            }
          });
        } else {
          this.errorMessage = response.message || this.t.translate('login.msg.failed');
          this.isLoading = false;
        }
      },
      error: (error) => {
  this.errorMessage = this.t.translate('login.msg.error');
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
      // JWT format: header.payload.signature
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const payloadJson = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
      const payload = JSON.parse(payloadJson);
      return payload["uid"] || payload["sub"] || null;
    } catch (e) {
      console.error('JWT decode error', e);
      return null;
    }
  }
}