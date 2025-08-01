import { Routes } from '@angular/router';
import { Search } from './components/search/search';
import { Profile } from './components/profile/profile';
import { LoginComponent } from './components/login/login';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'search', component: Search, canActivate: [AuthGuard] },
  { path: 'profile', component: Profile, canActivate: [AuthGuard] },
  { path: 'profil/:userId', component: Profile, canActivate: [AuthGuard] }
];
