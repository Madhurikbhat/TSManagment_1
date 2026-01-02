export interface Project {
  id?: string | number;
  code: string;
  projectName: string;
  clientName: string;
  isBillable: boolean;
  isActive: boolean;
  [key: string]: any;
}
export interface ProjectState {
  projects: Project[];
  loading: boolean;
  error: string | null;
}