import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    const token = this.authService.getToken();
    
    if (!token) {
      this.router.navigate(['/login']);
      return false;
    }

    // Token varsa ama süresi dolmuşsa profile'de popup gösterilsin
    if (this.authService.isTokenExpired()) {
      // Profile sayfasındaysa popup gösterilsin, değilse login'e yönlendir
      const currentUrl = this.router.url;
      if (!currentUrl.includes('/profile')) {
        this.router.navigate(['/login']);
        return false;
      }
    }

    return true;
  }
}
