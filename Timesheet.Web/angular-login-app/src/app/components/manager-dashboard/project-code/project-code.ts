import { CommonModule, KeyValuePipe } from '@angular/common';
import { Component, inject, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Manager } from '../../../services/manager';
import { Project, ProjectState } from './project-code.model';
import { Store } from '@ngrx/store';
import { createProjectCode, updateProjectCode } from '../../../../store/actions/project.actions';

@Component({
  selector: 'app-project-code',
  imports: [CommonModule, FormsModule],
  templateUrl: './project-code.html',
  styleUrl: './project-code.css',
})
export class ProjectCode implements OnInit {
  @Input() projectCodes: any[] = [];
  managerServ = inject(Manager);
  isEdit = false;
  editedIndex: number | null = null;
  columns: string[] = [];

  showCreateModal = false;
  newCode: Project = {
    code: '',
    projectName: '',
    clientName: '',
    isBillable: true,
    isActive: true
  };

  private store = inject(Store<{ projects: ProjectState }>);
  projects$ = this.store.select(state => state.projects.projects);
  loading$ = this.store.select(state => state.projects.loading)

  ngOnInit() {
    this.updateColumns();
  }

  ngOnChanges() {
  }

  private updateColumns() {
    const first = this.projectCodes && this.projectCodes.length ? this.projectCodes[0] : null;
    this.columns = first ? Object.keys(first) : ['Code', 'ProjectName', 'ClientName', 'IsBillable', 'IsActive'];
  }

  trackByIndex(index: number) {
    return index;
  }

  openProjectCode() {
    this.resetNew();
    this.isEdit = false;
    this.editedIndex = null;
    this.showCreateModal = true;
  }

  saveProjectCode() {
    const payload = { ...this.newCode };

    const id = payload.id;

    if (this.isEdit || id) {
      // update flow
      const updateId = id;
      this.store.dispatch(updateProjectCode({ id: updateId, payload }));
    } else {
      this.store.dispatch(createProjectCode({ payload }));

    }
    this.showCreateModal = false;
    this.isEdit = false;
    this.editedIndex = null;

  }
  editProjectCode(project: any, index?: number) {
    console.log('Editing project code:', project);
    this.newCode = { ...project };
    console.log('New code set to:', this.newCode);
    this.isEdit = true;
    this.editedIndex = (typeof index === 'number') ? index : this.projectCodes.indexOf(project);
    this.showCreateModal = true;
  }

  cancelCreate() {
    this.resetNew();
    this.showCreateModal = false;
  }

  private resetNew() {
    this.newCode = {
      code: '',
      projectName: '',
      clientName: '',
      isBillable: true,
      isActive: true
    };
  }

}
