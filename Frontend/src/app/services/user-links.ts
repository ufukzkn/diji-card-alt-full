import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDefinitionValue } from '../models/user-definition-value.model';

// DTO interfaces for API calls
export interface AddUserLinkRequest {
  userId: string;
  definitionId: number;
  value: string;
  sortId: number;
  customDefinitionName?: string;
}

export interface UpdateUserLinkRequest {
  id: number;
  userId: string;
  definitionId: number;
  value: string;
  sortId: number;
  customDefinitionName?: string;
}

@Injectable({ providedIn: 'root' })
export class UserLinksService {
  private api = 'http://localhost:5078/api/userdefinitionvalues';

  constructor(private http: HttpClient) { }

  getByUser(userId: string): Observable<UserDefinitionValue[]> {
    // path param kullan
    return this.http.get<UserDefinitionValue[]>(`${this.api}/${userId}`);
  }

  add(link: AddUserLinkRequest): Observable<UserDefinitionValue> {
    return this.http.post<UserDefinitionValue>(this.api, link);
  }

  addCustomDefinition(request: {
    userId: string;
    customDefinitionName: string;
    value: string;
    sortId: number;
  }): Observable<any> {
    return this.http.post<any>(`${this.api}/custom`, request);
  }

  deleteById(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }

  delete(userId: string, definitionId: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${userId}/${definitionId}`);
  }

  updateById(id: number, link: UpdateUserLinkRequest): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}`, link);
  }

  updateByIdOnly(id: number, payload: { value: string }): Observable<void> {
    return this.http.put<void>(`${this.api}/byid/${id}`, payload);
  }

  update(userId: string, definitionId: number, value: string) {
    return this.http.put<void>(
      `${this.api}/${userId}/${definitionId}`,
      { value }
    );
  }

  updateSortOrder(userLinks: UserDefinitionValue[]): Observable<void> {
    return this.http.put<void>(`${this.api}/sort`, userLinks);
  }
}
