export interface LinkDto {
  definitionName: string;
  value: string;
  sortId: number;
}

export interface UserProfile {
  userId: string;
  fullName?: string;
  company?: string;
  jobTitle?: string;
  email?: string;
  phoneNumber?: string;
  profilePhotoUrl?: string;
  links: LinkDto[];
}

export interface BasicUserInfo {
  userId: string;
  fullName?: string;
  company?: string;
  jobTitle?: string;
  email?: string;
  phoneNumber?: string;
  profilePhotoUrl?: string;
}

export interface FullProfileData {
  userId: string;
  fullName?: string;
  company?: string;
  jobTitle?: string;
  email?: string;
  phoneNumber?: string;
  profilePhotoUrl?: string;
  links: LinkDto[];
}

export interface PrivateProfileResponse {
  isPublic: boolean;
  accessGranted: boolean;
  message: string;
  profileData: UserProfile | BasicUserInfo | FullProfileData | null;
}

export interface PrivateProfileAccessRequest {
  authCode?: string;
  password?: string;
  accessToken?: string;
}

export interface UpdatePrivacySettingsRequest {
  isPublic: boolean;
  privateAccessPassword?: string;
}
