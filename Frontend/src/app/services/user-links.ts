import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDefinitionValue } from '../models/user-definition-value.model';

@Injectable({ providedIn: 'root' })
export class UserLinksService {
  private api = 'https://localhost:7220/api/userdefinitionvalues';

  constructor(private http: HttpClient) { }

  getByUser(userId: string): Observable<UserDefinitionValue[]> {
    // path param kullan
    return this.http.get<UserDefinitionValue[]>(`${this.api}/${userId}`);
  }



  add(link: UserDefinitionValue): Observable<UserDefinitionValue> {
    return this.http.post<UserDefinitionValue>(this.api, link);
  }

  delete(userId: string, definitionId: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${userId}/${definitionId}`);
  }

  update(userId: string, definitionId: number, value: string) {
    return this.http.put<void>(
      `${this.api}/${userId}/${definitionId}`,
      { userId, definitionId, value }
    );

  }

  updateSortOrder(userLinks: UserDefinitionValue[]): Observable<void> {
    return this.http.put<void>(`${this.api}/sort`, userLinks);
  }
}
