import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as ProjectActions from  '../actions/project.actions';
import { Manager } from '../../app/services/manager';

@Injectable()
export class ProjectEffects {
  private actions$ = inject(Actions);
  private managerServ = inject(Manager);

  loadProjects$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectActions.loadProjects),
      mergeMap(() =>
        this.managerServ.getProjectData().pipe(
          map(projects => ProjectActions.loadProjectsSuccess({ projects })),
          catchError(error => of(ProjectActions.loadProjectsFailure({ error: error.message })))
        )
      )
    )
  );
   createProjectCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectActions.createProjectCode),
      mergeMap(({ payload }) =>
        this.managerServ.createProjectCode(payload).pipe(
          map(project => ProjectActions.createProjectCodeSuccess({ project })),
          catchError(error => of(ProjectActions.createProjectCodeFailure({ error: error.message })))
        )
      )
    )
  );
  updateProjectCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectActions.updateProjectCode),
      mergeMap(({ id, payload }) =>
        this.managerServ.updateProjectCode(id, payload).pipe(
          map(project => ProjectActions.updateProjectCodeSuccess({ project })),
          catchError(error => of(ProjectActions.updateProjectCodeFailure({ error: error.message })))
        )
      )
    )
  );
}