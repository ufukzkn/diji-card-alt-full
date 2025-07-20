export interface LinkDto {
  definitionName: string;
  value: string;
}
export interface UserProfile {
  userId: string;
  links: LinkDto[];
}
