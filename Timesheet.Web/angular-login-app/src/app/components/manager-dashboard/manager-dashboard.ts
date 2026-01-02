import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Manager } from '../../services/manager';
import { ProjectCode } from './project-code/project-code';
import { ProjectAssignment } from './project-assignment/project-assignment';
import { TimesheetEntry } from './timesheet-entry/timesheet-entry';
import { Reports } from './reports/reports';
import { Store } from '@ngrx/store';
import { ProjectState } from './project-code/project-code.model';
import { loadProjects } from '../../../store/actions/project.actions';


@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ProjectCode, ProjectAssignment, TimesheetEntry, Reports],
  templateUrl: './manager-dashboard.html',
  styleUrls: ['./manager-dashboard.css'],
})
export class ManagerDashboard {
  private store = inject(Store<{ projects: ProjectState }>)

  managerServ =inject(Manager);

  projects$ = this.store.select(state => state.projects.projects);
  loading$ = this.store.select(state => state.projects.loading)

  currentTab: 'entry' | 'project-code' | 'report' | 'project-assignment' = 'entry';

  loadProjectCodes() {
    this.store.dispatch(loadProjects())
    // this.managerServ.getProjectData().subscribe((data: any) => {
    //   this.projectCodes = data;
    //   console.log('Project Codes:', this.projectCodes);
    // }, err => {
    //   console.error('Failed to load project codes', err);
    // });
  }
  selectTab(tab: 'entry' | 'project-code' | 'report' | 'project-assignment') {
    this.currentTab = tab;
    if (tab === 'project-code') {

      this.loadProjectCodes();
    }
  }
}
