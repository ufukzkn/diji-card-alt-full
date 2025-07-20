export interface LinkDto {
  definitionName: string;
  value: string;
  sortId: number;
}

export interface UserProfile {
  userId: string;
  links: LinkDto[];
}
