import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.models';   

export interface UserPreferences {
  isPublic?: boolean;
  viewMode?: string;
  gridColumns?: number;
  themeColor?: string;
  fontFamily?: string;
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = 'http://localhost:5078/api/user';  

  constructor(private http: HttpClient) { }

  getById(userId: string): Observable<User> {
    return this.http.get<User>(`${this.api}/${userId}`);
  }

  getPreferences(userId: string): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.api}/${userId}/preferences`);
  }

  updatePreferences(userId: string, preferences: UserPreferences): Observable<any> {
    return this.http.put(`${this.api}/${userId}/preferences`, preferences);
  }
}
