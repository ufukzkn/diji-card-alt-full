import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = 'https://localhost:7220/api/profile';
  constructor(private http: HttpClient) { }
  get(userId: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.api}/${userId}`);
  }
}
