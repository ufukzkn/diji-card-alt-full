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
    // Access token parametresi varsa, AuthGuard'ı atla (özel link erişimi)
    const accessToken = route.queryParams['access'];
    if (accessToken) {
      return true;
    }

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
