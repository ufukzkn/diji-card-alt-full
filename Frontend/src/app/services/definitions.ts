import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Definition } from '../models/definition.model';

@Injectable({ providedIn: 'root' })
export class DefinitionsService {
  private api = 'https://localhost:7220/api/definitions';

  constructor(private http: HttpClient) { }

  getAll(): Observable<Definition[]> {
    return this.http.get<Definition[]>(this.api);
  }

  add(definitionName: string): Observable<Definition> {
    return this.http.post<Definition>(this.api, { definitionName });
  }

}
