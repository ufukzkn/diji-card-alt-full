import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const accessToken = route.queryParams['access'];
    if (accessToken) return true; // özel link erişimi serbest

    const token = this.authService.getToken();

    // Profil rotası ise (örn: /profile/:id) anonim görüntülemeye izin ver
    const isProfileRoute = /\/profile\//.test(state.url) || route.routeConfig?.path?.includes('profile');
    if (!token) {
      if (isProfileRoute) {
        // Anonim kullanıcı profile bakabilir
        return true;
      }
      // Diğer korunan rotalar için login gerekli
      this.router.navigate(['/login']);
      return false;
    }

    // Token var ama expired ise: profile sayfasında popup gösterilmesine izin ver, diğerlerini login'e yönlendir
    if (this.authService.isTokenExpired()) {
      if (!isProfileRoute) {
        this.router.navigate(['/login']);
        return false;
      }
    }
    return true;
  }
}
