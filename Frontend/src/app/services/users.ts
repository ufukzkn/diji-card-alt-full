import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.models';   

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = 'https://localhost:7220/api/user';  

  constructor(private http: HttpClient) { }

  getById(userId: string): Observable<User> {
    return this.http.get<User>(`${this.api}/${userId}`);
  }
}
