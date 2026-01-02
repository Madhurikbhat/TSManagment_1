import { createReducer, on } from '@ngrx/store';
import { ProjectState } from '../../app/components/manager-dashboard/project-code/project-code.model';
import { createProjectCode, createProjectCodeFailure, createProjectCodeSuccess, loadProjects, loadProjectsFailure, loadProjectsSuccess, updateProjectCode, updateProjectCodeFailure, updateProjectCodeSuccess } from '../actions/project.actions';

export const initialState: ProjectState = {
  projects: [],
  loading: false,
  error: null
};

export const projectReducer = createReducer(
  initialState,
  on(loadProjects, (state) => ({ ...state, loading: true })),
  on(loadProjectsSuccess, (state, { projects  }) => ({ ...state, projects, loading: false })),
  on(loadProjectsFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(createProjectCode, (state) => ({ ...state, loading: true })),
  on(createProjectCodeSuccess, (state, { project  }) => ({ ...state, projects: [...state.projects, project], loading: false })),
  on(createProjectCodeFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(updateProjectCode, (state) => ({ ...state, loading: true })),
  on(updateProjectCodeSuccess, (state, { project  }) => {
    const key = (project as any).id ?? (project as any).code;
    const updatedProjects = state.projects.map(p => {
      const pKey = (p as any).id ?? (p as any).code;
      return pKey === key ? project : p;
    });
    return { ...state, projects: updatedProjects, loading: false };
  }),
  on(updateProjectCodeFailure, (state, { error }) => ({ ...state, error, loading: false }))

);