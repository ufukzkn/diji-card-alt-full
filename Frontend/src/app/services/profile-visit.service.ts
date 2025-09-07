import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProfileVisitService {
  private readonly api = 'http://localhost:5078/api/profile-visits';

  constructor(private http: HttpClient) {}

  logVisit(profileUserId: string, isSpecialAccess: boolean): Observable<any> {
    return this.http.post(this.api, { profileUserId, isSpecialAccess });
  }
}
