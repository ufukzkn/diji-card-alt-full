import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile, PrivateProfileResponse, PrivateProfileAccessRequest, UpdatePrivacySettingsRequest, BasicUserInfo } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly api = 'http://localhost:5078/api/profile';

  constructor(private http: HttpClient) { }

  get(userId: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.api}/${userId}`);
  }

  // Private profil için yeni method
  getWithPrivacyCheck(userId: string): Observable<PrivateProfileResponse> {
    return this.http.get<PrivateProfileResponse>(`${this.api}/${userId}`);
  }

  // Basic info güvenli endpoint
  getBasicInfo(userId: string): Observable<PrivateProfileResponse> {
    return this.http.get<PrivateProfileResponse>(`${this.api}/${userId}/basic-info`);
  }

  verifyPrivateAccess(userId: string, request: PrivateProfileAccessRequest): Observable<PrivateProfileResponse> {
    return this.http.post<PrivateProfileResponse>(`${this.api}/${userId}/verify-access`, request);
  }

  // Şifre doğrulandıktan sonra tam profil (basic + links) al
  verifyPrivateAccessFull(userId: string, request: PrivateProfileAccessRequest): Observable<PrivateProfileResponse> {
    return this.http.post<PrivateProfileResponse>(`${this.api}/${userId}/verify-access-full`, request);
  }

  updatePrivacySettings(userId: string, settings: UpdatePrivacySettingsRequest): Observable<any> {
    return this.http.put(`${this.api}/${userId}/privacy-settings`, settings);
  }

  createSpecialLink(userId: string, request: any): Observable<any> {
    return this.http.post(`${this.api}/${userId}/create-special-link`, request);
  }

  getSpecialLinks(userId: string): Observable<any> {
    return this.http.get(`${this.api}/${userId}/special-links`);
  }

  deleteSpecialLink(userId: string, linkId: number): Observable<any> {
    return this.http.delete(`${this.api}/${userId}/special-links/${linkId}`);
  }

  uploadPhoto(userId: string, formData: FormData): Observable<any> {
    return this.http.post(`${this.api}/${userId}/photo`, formData);
  }

  deletePhoto(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${userId}/photo`);
  }
}
