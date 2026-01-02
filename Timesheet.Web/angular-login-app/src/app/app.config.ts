import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
 import { provideStore } from '@ngrx/store';
 import { provideEffects } from '@ngrx/effects';
// import { reducers } from '../store';
 import { projectReducer } from '../store/reducers/project.reducers';
 import { ProjectEffects } from '../store/effects/project.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
    provideStore({ projects: projectReducer }),
    provideEffects([ProjectEffects])
]
};
