export interface LoginRequest {
  kullaniciAdi: string;
  sifre: string;
  authCode: string;
  applicationId: string;
  redirectUrl: string;
  requestId: string;
  source: string;
  isShowExceptionMessage: boolean;
}

export interface OAuthTokenRequest {
  requestId: string;
  authToken: string;
  redirectUrl: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  requestId: string;
  authToken: string;
  redirectUrl: string;
}

export interface TokenResponse {
  success: boolean;
  message: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
}
