import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly api = 'https://localhost:7220/api/profile';

  constructor(private http: HttpClient) { }

  get(userId: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.api}/${userId}`);
  }

  uploadPhoto(userId: string, formData: FormData): Observable<any> {
    return this.http.post(`${this.api}/${userId}/photo`, formData);
  }

  deletePhoto(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${userId}/photo`);
  }
}
