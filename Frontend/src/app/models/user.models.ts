export interface User {
  userId: string;
  fullName: string;
  company?: string;
  email?: string;
  phoneNumber?: string;
  jobTitle?: string;
  isPublic?: boolean;
  // backend’de varsa diğer sabit alanlar
}
