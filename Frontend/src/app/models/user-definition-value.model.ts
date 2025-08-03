import { Definition } from './definition.model';   
export interface UserDefinitionValue {
  id: number; // Auto-increment ID
  userId: string;
  definitionId: number;
  value: string;
  sortId: number;
  customDefinitionName?: string; // Custom definition name (nullable)
  displayName: string; // Frontend için gösterilecek isim
  isCustom: boolean; // Helper property
  definition?: Definition;   
}
