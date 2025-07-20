import { Definition } from './definition.model';   
export interface UserDefinitionValue {
  userId: string;
  definitionId: number;
  value: string;
  definition?: Definition;   
}
