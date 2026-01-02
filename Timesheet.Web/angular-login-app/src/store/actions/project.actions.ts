import { createAction, props } from '@ngrx/store';
import { Project } from '../../app/components/manager-dashboard/project-code/project-code.model';

export const loadProjects = createAction('[Project] Load Projects');
export const loadProjectsSuccess = createAction(
  '[Project API] Load Projects Success',
  props<{ projects: any }>()
);
export const loadProjectsFailure = createAction(
  '[Project API] Load Projects Failure',
  props<{ error: string }>()
);
export const createProjectCode = createAction(
  '[Project API] Create Project Code',
  props<{ payload: Project }>()
);

// create success (API returned created project)
export const createProjectCodeSuccess = createAction(
  '[Project API] Create Project Code Success',
  props<{ project: any }>()
);

// create failure (API error)
export const createProjectCodeFailure = createAction(
  '[Project API] Create Project Code Failure',
  props<{ error: any }>()
);

export const updateProjectCode = createAction(
  '[Project API] Update Project Code',
  props<{ id: number | string | undefined, payload: Project }>()
);

// update success (API returned updated project)
export const updateProjectCodeSuccess = createAction(
  '[Project API] Update Project Code Success',
  props<{ project: any }>()
);

// update failure (API error)
export const updateProjectCodeFailure = createAction(
  '[Project API] Update Project Code Failure',
  props<{ error: any }>()
);