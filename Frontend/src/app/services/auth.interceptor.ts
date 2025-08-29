import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

// Global Auth Interceptor: Adds Authorization header and handles 401/session timeout
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getToken();

  let authReq = req;
  // Do not attach for auth endpoints
  if (token && !/\/api\/Auth\//i.test(req.url)) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        const msg = (err.error?.message || err.error?.Message || '').toString().toLowerCase();
        // If token expired / session timeout
        if (msg.includes('zaman') || msg.includes('expire') || msg.includes('timeout')) {
          // Token süresi dolmuşsa direkt temizle (modal olmadan)
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('tokenExpiry');
          // Navigate to login with query param for informing user
          router.navigate(['/login'], { queryParams: { timeout: '1' } });
        }
      }
      return throwError(() => err);
    })
  );
};
