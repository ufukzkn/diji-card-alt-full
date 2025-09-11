import { Injectable } from '@angular/core';
import { environment } from '../environments';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProfileVisitService {
  private readonly api = environment.apiBase + '/api/profile-visits';

  constructor(private http: HttpClient) {}

  logVisit(profileUserId: string, isSpecialAccess: boolean): Observable<any> {
    return this.http.post(this.api, { profileUserId, isSpecialAccess });
  }
}
