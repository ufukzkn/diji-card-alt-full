import { Routes } from '@angular/router';
import { Search } from './components/search/search';
import { Profile } from './components/profile/profile';



export const routes: Routes = [
  { path: '', component: Search },
  { path: 'profil/:userId', component: Profile }
];
