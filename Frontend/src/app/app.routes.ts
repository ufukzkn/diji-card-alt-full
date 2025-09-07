import { Routes } from '@angular/router';
import { Search } from './components/search/search';
import { Profile } from './components/profile/profile';
import { LoginComponent } from './components/login/login';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'search', component: Search, canActivate: [AuthGuard] },
  // Public profile viewing (both /profile/:userId and legacy /profil/:userId)
  { path: 'profile/:userId', component: Profile },
  { path: 'profil/:userId', component: Profile },
  // Optional: base /profile without id redirects to search or login
  { path: 'profile', redirectTo: '/search', pathMatch: 'full' }
];
